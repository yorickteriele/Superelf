using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace Superelf.Application;

/// <summary>
/// Interface for managing player images, including downloading and cleanup operations.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Downloads an image from a URL and saves it to local storage with a sanitized filename.
    /// </summary>
    /// <param name="imageUrl">URL of the image to download</param>
    /// <param name="playerName">Name of the player (used for filename generation)</param>
    /// <returns>Local file path for the saved image, or null if download failed</returns>
    Task<string?> DownloadAndSaveImageAsync(string? imageUrl, string playerName);

    /// <summary>
    /// Deletes an image file from local storage.
    /// </summary>
    /// <param name="imagePath">Path to the image file to delete</param>
    /// <returns>True if deletion was successful or file doesn't exist, false on error</returns>
    Task<bool> DeleteImageAsync(string? imagePath);
}

/// <summary>
/// Service for managing player images, handling download, storage, and cleanup operations.
/// Supports configurable storage locations and ensures proper file system permissions.
/// </summary>
public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _imagesDirectory;

    /// <summary>
    /// Initializes a new instance of the ImageService.
    /// </summary>
    /// <param name="logger">Logger for image operations</param>
    /// <param name="configuration">Configuration for image storage settings</param>
    /// <param name="httpClient">HTTP client for downloading images</param>
    /// <exception cref="Exception">Thrown if image directory initialization fails</exception>
    public ImageService(ILogger<ImageService> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        
        try
        {
            // Get images directory from configuration or use default
            var configuredPath = _configuration["ImageStorage:Directory"];
            if (!string.IsNullOrEmpty(configuredPath))
            {
                _imagesDirectory = configuredPath;
            }
            else
            {
                // Fallback to a local images directory in the current working directory
                _imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "images", "players");
            }
            
            // Ensure the directory exists with proper permissions
            EnsureDirectoryExists(_imagesDirectory);
            _logger.LogInformation("ImageService initialized with directory: {Directory}", _imagesDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ImageService");
            throw;
        }
    }

    public async Task<string?> DownloadAndSaveImageAsync(string? imageUrl, string playerName)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        try
        {
            // Skip if it's already a local path
            if (imageUrl.StartsWith("/") || imageUrl.StartsWith("images/"))
                return imageUrl;

            // Create a safe filename from player name
            var safePlayerName = CreateSafeFileName(playerName);
            var fileExtension = GetFileExtensionFromUrl(imageUrl);
            var fileName = $"{safePlayerName}_{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(_imagesDirectory, fileName);

            // Download the image
            var response = await _httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download image from {ImageUrl}: {StatusCode}", imageUrl, response.StatusCode);
                return null;
            }

            // Save the image
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await response.Content.CopyToAsync(fileStream);

            _logger.LogInformation("Successfully downloaded and saved image for player {PlayerName} to {FilePath}", playerName, filePath);

            // Return the relative path that will be accessible via web (from React app's public folder), URL-encoded
            var encodedFileName = Uri.EscapeDataString(fileName);
            return $"/images/players/{encodedFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image from {ImageUrl} for player {PlayerName}", imageUrl, playerName);
            return null;
        }
    }

    public async Task<bool> DeleteImageAsync(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return true;

        try
        {
            // Convert web path to file system path
            var relativePath = imagePath.TrimStart('/');
            string fullPath;
            
            if (relativePath.StartsWith("images/"))
            {
                // Path is relative to the Web project's public folder
                var currentDir = Directory.GetCurrentDirectory();
                fullPath = Path.Combine(currentDir, "..", "Superelf.Web", "public", relativePath);
            }
            else
            {
                // Assume it's relative to the images directory
                fullPath = Path.Combine(_imagesDirectory, Path.GetFileName(relativePath));
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted image file: {FilePath}", fullPath);
                return true;
            }

            return true; // File doesn't exist, consider it successfully "deleted"
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image file: {ImagePath}", imagePath);
            return false;
        }
    }

    /// <summary>
    /// Ensures that a directory exists, creating it if necessary.
    /// When on Unix-like systems, sets appropriate file permissions.
    /// </summary>
    /// <param name="path">The directory path to ensure exists</param>
    /// <exception cref="Exception">Thrown if directory creation or permission setting fails</exception>
    private void EnsureDirectoryExists(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation("Created directory: {Path}", path);
                
                // For Unix-like systems, try to set appropriate directory permissions
                #if !WINDOWS
                if (Environment.OSVersion.Platform == PlatformID.Unix || 
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    try
                    {
                        // This requires .NET 6.0 or later
                        System.IO.File.SetUnixFileMode(path, 
                            UnixFileMode.UserRead | 
                            UnixFileMode.UserWrite | 
                            UnixFileMode.UserExecute |
                            UnixFileMode.GroupRead |
                            UnixFileMode.GroupExecute |
                            UnixFileMode.OtherRead |
                            UnixFileMode.OtherExecute);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        // Ignore on platforms that don't support UnixFileMode
                        _logger.LogDebug("UnixFileMode not supported on this platform");
                    }
                }
                #endif
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or access directory: {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Creates a safe filename from a player name by removing invalid characters
    /// and limiting the length. Falls back to a GUID if no name provided.
    /// </summary>
    /// <param name="playerName">The player name to convert to a safe filename</param>
    /// <returns>A safe filename without invalid characters and reasonable length</returns>
    private static string CreateSafeFileName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return Guid.NewGuid().ToString("N");
            
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = playerName;
        
        foreach (var invalidChar in invalidChars)
        {
            safeName = safeName.Replace(invalidChar, '_');
        }

        // Limit length and trim
        return safeName.Trim().Substring(0, Math.Min(safeName.Length, 50));
    }

    /// <summary>
    /// Extracts the file extension from a URL's path. Defaults to .jpg if
    /// no valid extension is found.
    /// </summary>
    /// <param name="url">The URL to extract extension from</param>
    /// <returns>The file extension including the dot (e.g., ".jpg")</returns>
    private static string GetFileExtensionFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var extension = Path.GetExtension(path);
            
            // Default to .jpg if no extension found
            return string.IsNullOrEmpty(extension) ? ".jpg" : extension;
        }
        catch
        {
            return ".jpg"; // Default fallback
        }
    }
}
