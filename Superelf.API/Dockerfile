
# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY . .
WORKDIR "/src/Superelf.API"
RUN dotnet build "Superelf.API.csproj" -c Release -o /app/build

# Stage 3: Publish image
FROM build AS publish
RUN dotnet publish "Superelf.API.csproj" -c Release -o /app/publish

# Stage 4: Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN addgroup --system --gid 1001 superelf && \
    adduser --system --uid 1001 --gid 1001 superelf
RUN chown -R superelf:superelf /app
USER superelf

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Superelf.API.dll"]