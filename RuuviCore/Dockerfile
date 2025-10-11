# Build stage with caching optimization
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim-arm32v7 AS build
WORKDIR /src

# Copy only csproj files first for better layer caching
COPY ["RuuviCore/RuuviCore.csproj", "RuuviCore/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["FileStorageProvider/FileStorageProvider.csproj", "FileStorageProvider/"]
COPY ["GrainInterfaces/GrainInterfaces.csproj", "GrainInterfaces/"]
COPY ["GrainServices/GrainServices.csproj", "GrainServices/"]
COPY ["Grains/Grains.csproj", "Grains/"]

# Restore dependencies as a separate layer
RUN dotnet restore "RuuviCore/RuuviCore.csproj" --runtime linux-arm

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/RuuviCore"
RUN dotnet build "RuuviCore.csproj" \
    -c Release \
    --no-restore \
    -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RuuviCore.csproj" \
    -c Release \
    --no-restore \
    --no-build \
    --runtime linux-arm \
    --self-contained false \
    -o /app/publish

# Runtime stage - smaller base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim-arm32v7 AS final

# Install required libraries for Bluetooth/D-Bus support
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libc6 \
        libgcc-s1 \
        libstdc++6 \
        dbus \
        bluez && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -g 1000 ruuviapp && \
    useradd -r -u 1000 -g ruuviapp ruuviapp && \
    usermod -aG bluetooth ruuviapp

WORKDIR /app

# Copy published application
COPY --from=publish --chown=ruuviapp:ruuviapp /app/publish .

# Set environment variables for better performance
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

# Switch to non-root user
USER ruuviapp

EXPOSE 8080
EXPOSE 443

# Use exec form for better signal handling
ENTRYPOINT ["dotnet", "ruuvi-core.dll"]