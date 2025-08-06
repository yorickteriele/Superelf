# Use the runtime image directly - no building needed
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install PostgreSQL client and curl (for health checks)
RUN apt-get update && apt-get install -y postgresql-client curl && rm -rf /var/lib/apt/lists/*

# Copy pre-built published files (built by GitHub Actions)
COPY /Superelf.API/bin/Release/net8.0/publish/ .

# Create a non-root user
RUN addgroup --system --gid 1001 superelf && \
    adduser --system --uid 1001 --gid 1001 superelf

# Change ownership of the app directory
RUN chown -R superelf:superelf /app

USER superelf

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Superelf.API.dll"]