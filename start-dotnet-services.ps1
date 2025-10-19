# Simple .NET Services Startup Script for Windows
# This script starts all .NET services with the dual authentication architecture

Write-Host "🚀 Starting Booking Platform .NET Services" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Check if databases are running
Write-Host "📦 Checking infrastructure services..." -ForegroundColor Yellow
$containers = docker ps --format "table {{.Names}}" | Select-String "postgres|redis"
if ($containers.Count -lt 4) {
    Write-Host "❌ Required infrastructure services are not running. Please start them first:" -ForegroundColor Red
    Write-Host "   docker-compose -f docker-compose.stable.yml up -d postgres-inventory postgres-booking postgres-user redis" -ForegroundColor White
    exit 1
}

Write-Host "✅ Infrastructure services are running" -ForegroundColor Green

Write-Host "🔐 Starting IdentityServer..." -ForegroundColor Yellow
$identityJob = Start-Job -ScriptBlock {
    Set-Location "src/IdentityServer"
    dotnet run --urls="https://localhost:5005"
}

Write-Host "⏳ Waiting for IdentityServer to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

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
Start-Sleep -Seconds 15

Write-Host ""
Write-Host "✅ All services started successfully!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host "🔐 IdentityServer:     https://localhost:5005" -ForegroundColor Cyan
Write-Host "🌐 API Gateway:        https://localhost:5000" -ForegroundColor Cyan
Write-Host "👤 User Service:       https://localhost:5004" -ForegroundColor Cyan
Write-Host "📅 Booking Service:    https://localhost:5001" -ForegroundColor Cyan
Write-Host "📦 Inventory Service:  https://localhost:5002" -ForegroundColor Cyan
Write-Host "💳 Payment Service:    https://localhost:5003" -ForegroundColor Cyan
Write-Host ""
Write-Host "🧪 Test the dual authentication:" -ForegroundColor Yellow
Write-Host "   1. Use 'dual-auth-tests.http' file for testing" -ForegroundColor White
Write-Host "   2. First get service token from IdentityServer" -ForegroundColor White
Write-Host "   3. Then test user authentication through Gateway" -ForegroundColor White
Write-Host "   4. Finally test service-to-service communication" -ForegroundColor White
Write-Host ""
Write-Host "🛑 To stop all services:" -ForegroundColor Red
Write-Host "   - Press Ctrl+C to stop this script" -ForegroundColor White
Write-Host ""

# Function to cleanup on exit
function Cleanup {
    Write-Host ""
    Write-Host "🛑 Stopping services..." -ForegroundColor Yellow
    
    # Stop all jobs
    Stop-Job $identityJob, $gatewayJob, $userJob, $bookingJob, $inventoryJob, $paymentJob -ErrorAction SilentlyContinue
    Remove-Job $identityJob, $gatewayJob, $userJob, $bookingJob, $inventoryJob, $paymentJob -ErrorAction SilentlyContinue
    
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
