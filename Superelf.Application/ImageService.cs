using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace Superelf.Application;

public interface IImageService
{
    Task<string?> DownloadAndSaveImageAsync(string? imageUrl, string playerName);
    Task<bool> DeleteImageAsync(string? imagePath);
}

public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _imagesDirectory;

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

    private void EnsureDirectoryExists(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation("Created directory: {Path}", path);
                
                // Set appropriate permissions if on Unix
                if (Environment.OSVersion.Platform == PlatformID.Unix || 
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    try
                    {
                        // This requires .NET 6.0 or later
                        File.SetUnixFileMode(path, 
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
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or access directory: {Path}", path);
            throw;
        }
    }

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
