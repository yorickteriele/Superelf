
# Stage 1: Build and publish the .NET app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for better layer caching
COPY Superelf.Web.sln ./
COPY Superelf.API/Superelf.API.csproj Superelf.API/
COPY Superelf.Application/Superelf.Application.csproj Superelf.Application/
COPY Superelf.Domain/Superelf.Domain.csproj Superelf.Domain/
COPY Superelf.Infrastructure/Superelf.Infrastructure.csproj Superelf.Infrastructure/

# Restore dependencies
RUN dotnet restore Superelf.Web.sln

# Copy the rest of the source code
COPY . .

# Publish the application
RUN dotnet publish Superelf.API/Superelf.API.csproj -c Release -o /app/publish --no-restore

# Stage 2: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install PostgreSQL client and curl (for health checks)
RUN apt-get update && apt-get install -y postgresql-client curl && rm -rf /var/lib/apt/lists/*

# Copy published files from build stage
COPY --from=build /app/publish .

# Create a non-root user
RUN addgroup --system --gid 1001 superelf && \
    adduser --system --uid 1001 --gid 1001 superelf
RUN chown -R superelf:superelf /app
USER superelf

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Superelf.API.dll"]
