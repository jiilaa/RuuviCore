# Build stage with caching optimization
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine3.23-arm32v7 AS build
WORKDIR /src

# Copy only csproj files first for better layer caching
COPY ["RuuviCore/RuuviCore.csproj", "RuuviCore/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["FileStorageProvider/FileStorageProvider.csproj", "FileStorageProvider/"]
COPY ["GrainInterfaces/GrainInterfaces.csproj", "GrainInterfaces/"]
COPY ["GrainServices/GrainServices.csproj", "GrainServices/"]
COPY ["Grains/Grains.csproj", "Grains/"]
COPY ["Protocol/Protocol.csproj", "Protocol/"]
COPY ["RuuviUI/RuuviUI.csproj", "RuuviUI/"]

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
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.23-arm32v7 AS final

# Install required libraries for Bluetooth/D-Bus support
RUN apk add --no-cache \
        dbus \
        bluez \
        glib

# Create non-root user for security (Alpine uses addgroup/adduser)
RUN addgroup -g 1000 ruuviapp && \
    adduser -D -u 1000 -G ruuviapp ruuviapp && \
    addgroup -S bluetooth && \
    addgroup ruuviapp bluetooth

WORKDIR /app

# Copy published application
COPY --from=publish --chown=ruuviapp:ruuviapp /app/publish .

# Set environment variables for better performance
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:4000 \
    ASPNETCORE_ENVIRONMENT=Production

# Switch to non-root user
USER ruuviapp

EXPOSE 4000

# Use exec form for better signal handling, enable HTTP gateway for UI
ENTRYPOINT ["dotnet", "ruuvi-core.dll", "--http"]