# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY Capitol_Theatre/Capitol_Theatre.csproj ./
COPY nuget.config ./nuget.config
RUN dotnet restore "Capitol_Theatre.csproj"

# Copy the rest of the app and build
COPY Capitol_Theatre/. ./
RUN dotnet build "Capitol_Theatre.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Capitol_Theatre.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ensure upload folder exists
RUN mkdir -p wwwroot/images/upload && chmod -R 777 wwwroot/images/upload

ENTRYPOINT ["dotnet", "Capitol_Theatre.dll"]
