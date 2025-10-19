# Dual Authentication Architecture Startup Script for Windows
# This script starts all services with the new two-level authentication system

Write-Host "🚀 Starting Booking Platform with Dual Authentication Architecture" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker first." -ForegroundColor Red
    exit 1
}

Write-Host "📦 Starting infrastructure services..." -ForegroundColor Yellow
docker-compose up -d postgres-inventory postgres-booking postgres-user sqlserver-payment mongodb-review mongodb-analytics rabbitmq redis elasticsearch consul jaeger prometheus grafana

Write-Host "⏳ Waiting for infrastructure services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "🔐 Starting IdentityServer..." -ForegroundColor Yellow
docker-compose up -d identityserver

Write-Host "⏳ Waiting for IdentityServer to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

Write-Host "🌐 Starting API Gateway..." -ForegroundColor Yellow
$gatewayJob = Start-Job -ScriptBlock {
    Set-Location "src/ApiGateway/ApiGateway.Ocelot"
    dotnet run --urls="https://localhost:5000"
}

Write-Host "👤 Starting User Service..." -ForegroundColor Yellow
$userJob = Start-Job -ScriptBlock {
    Set-Location "src/Services/User/User.API"
    dotnet run --urls="https://localhost:5004"
}

Write-Host "📅 Starting Booking Service..." -ForegroundColor Yellow
$bookingJob = Start-Job -ScriptBlock {
    Set-Location "src/Services/Booking/Booking.API"
    dotnet run --urls="https://localhost:5001"
}

Write-Host "📦 Starting Inventory Service..." -ForegroundColor Yellow
$inventoryJob = Start-Job -ScriptBlock {
    Set-Location "src/Services/Inventory/Inventory.API"
    dotnet run --urls="https://localhost:5002"
}

Write-Host "💳 Starting Payment Service..." -ForegroundColor Yellow
$paymentJob = Start-Job -ScriptBlock {
    Set-Location "src/Services/Payment/Payment.API"
    dotnet run --urls="https://localhost:5003"
}

Write-Host "⏳ Waiting for all services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

Write-Host ""
Write-Host "✅ All services started successfully!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host "🔐 IdentityServer:     https://localhost:5005" -ForegroundColor Cyan
Write-Host "🌐 API Gateway:        https://localhost:5000" -ForegroundColor Cyan
Write-Host "👤 User Service:       https://localhost:5004" -ForegroundColor Cyan
Write-Host "📅 Booking Service:    https://localhost:5001" -ForegroundColor Cyan
Write-Host "📦 Inventory Service:  https://localhost:5002" -ForegroundColor Cyan
Write-Host "💳 Payment Service:    https://localhost:5003" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Monitoring:" -ForegroundColor Magenta
Write-Host "   - Jaeger UI:        http://localhost:16686" -ForegroundColor White
Write-Host "   - Prometheus:       http://localhost:9090" -ForegroundColor White
Write-Host "   - Grafana:          http://localhost:3000 (admin/admin)" -ForegroundColor White
Write-Host "   - RabbitMQ UI:      http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
Write-Host "🧪 Test the dual authentication:" -ForegroundColor Yellow
Write-Host "   1. Use 'dual-auth-tests.http' file for testing" -ForegroundColor White
Write-Host "   2. First get service token from IdentityServer" -ForegroundColor White
Write-Host "   3. Then test user authentication through Gateway" -ForegroundColor White
Write-Host "   4. Finally test service-to-service communication" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop all services:" -ForegroundColor Red
Write-Host "   - Press Ctrl+C to stop this script" -ForegroundColor White
Write-Host "   - Run: docker-compose down" -ForegroundColor White
Write-Host ""

# Function to cleanup on exit
function Cleanup {
    Write-Host ""
    Write-Host "🛑 Stopping services..." -ForegroundColor Yellow
    
    # Stop all jobs
    Stop-Job $gatewayJob, $userJob, $bookingJob, $inventoryJob, $paymentJob -ErrorAction SilentlyContinue
    Remove-Job $gatewayJob, $userJob, $bookingJob, $inventoryJob, $paymentJob -ErrorAction SilentlyContinue
    
    # Stop Docker containers
    docker-compose down
    
    Write-Host "✅ All services stopped." -ForegroundColor Green
    exit 0
}

# Set trap to cleanup on script exit
$null = Register-EngineEvent PowerShell.Exiting -Action { Cleanup }

# Keep script running
Write-Host "🔄 Services are running. Press Ctrl+C to stop all services." -ForegroundColor Green
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
} catch {
    Cleanup
}
