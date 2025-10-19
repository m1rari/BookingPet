# Stable Infrastructure Startup Script for Windows
# This script starts infrastructure services step by step to avoid Docker Hub issues

Write-Host "🚀 Starting Booking Platform Infrastructure (Stable Version)" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Green

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker first." -ForegroundColor Red
    exit 1
}

Write-Host "📦 Starting databases..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d postgres-inventory postgres-booking postgres-user sqlserver-payment

Write-Host "⏳ Waiting for databases to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

Write-Host "📦 Starting NoSQL databases..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d mongodb-review mongodb-analytics

Write-Host "📦 Starting message broker..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d rabbitmq

Write-Host "📦 Starting cache..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d redis

Write-Host "⏳ Waiting for core services..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "📦 Starting search engine..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d elasticsearch

Write-Host "📦 Starting service discovery..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d consul

Write-Host "⏳ Waiting for infrastructure services..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "📦 Starting observability stack..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d jaeger prometheus grafana

Write-Host "⏳ Waiting for observability services..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "🔐 Starting IdentityServer..." -ForegroundColor Yellow
docker-compose -f docker-compose.stable.yml up -d identityserver

Write-Host "⏳ Waiting for IdentityServer to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host ""
Write-Host "✅ Infrastructure services started successfully!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host "🔐 IdentityServer:     https://localhost:5005" -ForegroundColor Cyan
Write-Host "📊 Monitoring:" -ForegroundColor Magenta
Write-Host "   - Jaeger UI:        http://localhost:16686" -ForegroundColor White
Write-Host "   - Prometheus:       http://localhost:9090" -ForegroundColor White
Write-Host "   - Grafana:          http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "   - RabbitMQ UI:      http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Now you can start the .NET services manually:" -ForegroundColor Yellow
Write-Host "   - API Gateway:      dotnet run --project src/ApiGateway/ApiGateway.Ocelot" -ForegroundColor White
Write-Host "   - User Service:     dotnet run --project src/Services/User/User.API" -ForegroundColor White
Write-Host "   - Booking Service:  dotnet run --project src/Services/Booking/Booking.API" -ForegroundColor White
Write-Host "   - Inventory Service: dotnet run --project src/Services/Inventory/Inventory.API" -ForegroundColor White
Write-Host "   - Payment Service:  dotnet run --project src/Services/Payment/Payment.API" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop infrastructure: docker-compose -f docker-compose.stable.yml down" -ForegroundColor Red
Write-Host ""

# Function to cleanup on exit
function Cleanup {
    Write-Host ""
    Write-Host "🛑 Stopping infrastructure services..." -ForegroundColor Yellow
    docker-compose -f docker-compose.stable.yml down
    Write-Host "✅ Infrastructure stopped." -ForegroundColor Green
    exit 0
}

# Set trap to cleanup on script exit
$null = Register-EngineEvent PowerShell.Exiting -Action { Cleanup }

# Keep script running
Write-Host "🔄 Infrastructure is running. Press Ctrl+C to stop." -ForegroundColor Green
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
} catch {
    Cleanup
}
