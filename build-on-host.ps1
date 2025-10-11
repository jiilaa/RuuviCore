# Build .NET app on Windows host, then create ARM Docker image
param(
    [string]$Registry = "raspberrypi",
    [string]$ImageName = "ruuvicore",
    [string]$Tag = "latest"
)

$registryUrl = "${Registry}:5000"
$fullImageName = "${registryUrl}/${ImageName}:${Tag}"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Host Build + ARM Docker Image" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Build .NET application on Windows for linux-arm
Write-Host "`nStep 1: Building .NET application for linux-arm..." -ForegroundColor Yellow

# Clean previous build
if (Test-Path ".\publish") {
    Remove-Item -Path ".\publish" -Recurse -Force
}

# Build the application for ARM32
Write-Host "Running dotnet publish for linux-arm..." -ForegroundColor Gray
dotnet publish RuuviCore/RuuviCore.csproj `
    -c Release `
    -r linux-arm `
    --self-contained false `
    -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ .NET build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ .NET application built successfully" -ForegroundColor Green

# Step 2: Build Docker image with pre-built application
Write-Host "`nStep 2: Setting up Docker buildx with HTTP registry config..." -ForegroundColor Yellow

# Remove old builder if exists
docker buildx rm hostbuilder 2>$null | Out-Null

# Get the absolute path to the config file
$configPath = (Get-Item ".\buildkitd.toml").FullName
Write-Host "Using buildkit config: $configPath" -ForegroundColor Gray

# Create buildx builder with custom config for HTTP registry
docker buildx create `
    --name hostbuilder `
    --driver docker-container `
    --buildkitd-flags "--allow-insecure-entitlement network.host" `
    --config "$configPath" `
    --use

# Bootstrap the builder
docker buildx inspect --bootstrap | Out-Null

Write-Host "✓ Buildx configured for HTTP registry" -ForegroundColor Green

# Step 3: Build and push the Docker image
Write-Host "`nStep 3: Building ARM32 Docker image..." -ForegroundColor Yellow
Write-Host "Building Docker image with pre-built application..." -ForegroundColor Gray
docker buildx build `
    --platform linux/arm/v7 `
    -f RuuviCore/Dockerfile.simplified `
    -t $fullImageName `
    --push `
    --allow network.host `
    .

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host " ✓ Build and push successful!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green

    Write-Host "`nOn your Raspberry Pi, run:" -ForegroundColor Yellow
    Write-Host "  docker pull localhost:5000/${ImageName}:${Tag}" -ForegroundColor Cyan
    Write-Host "  docker-compose -f docker-compose-local-registry.yml up -d" -ForegroundColor Cyan
} else {
    Write-Host "✗ Docker build failed!" -ForegroundColor Red
    exit 1
}

# Cleanup
Write-Host "`nCleaning up build artifacts..." -ForegroundColor Gray
Remove-Item -Path ".\publish" -Recurse -Force