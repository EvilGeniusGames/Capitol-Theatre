# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
#USER $APP_UID
USER root
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Capitol_Theatre/Capitol_Theatre.csproj", "Capitol_Theatre/"]
RUN dotnet restore "./Capitol_Theatre/Capitol_Theatre.csproj"
COPY . .
WORKDIR "/src/Capitol_Theatre"
RUN dotnet build "./Capitol_Theatre.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Capitol_Theatre.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final

# Set timezone
RUN apt-get update && apt-get install -y tzdata && \
    ln -sf /usr/share/zoneinfo/America/Yellowknife /etc/localtime && \
    echo "America/Yellowknife" > /etc/timezone && \
    dpkg-reconfigure -f noninteractive tzdata

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["sh", "-c", "ln -sf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone && exec dotnet Capitol_Theatre.dll"]

