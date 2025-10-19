# ========================================================================
#  🚀 BOOKING PLATFORM - QUICK START SCRIPT (PowerShell)
# ========================================================================

Write-Host "📋 Запуск всех микросервисов и API Gateway..." -ForegroundColor Cyan
Write-Host ""

# Функция для запуска сервиса
function Start-Service {
    param(
        [string]$Name,
        [string]$Path,
        [int]$Port
    )
    
    Write-Host "🔧 Запуск $Name (Port $Port)..." -ForegroundColor Yellow
    
    $command = "cd '$Path' && dotnet run --urls http://localhost:$Port"
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command -WindowStyle Normal
}

# Запуск всех сервисов
Start-Service "Inventory Service" "src\Services\Inventory\Inventory.API" 5001
Start-Sleep -Seconds 2

Start-Service "Booking Service" "src\Services\Booking\Booking.API" 5002
Start-Sleep -Seconds 2

Start-Service "User Service" "src\Services\User\User.API" 5003
Start-Sleep -Seconds 2

Start-Service "Payment Service" "src\Services\Payment\Payment.API" 5004
Start-Sleep -Seconds 2

Start-Service "API Gateway" "src\ApiGateway\ApiGateway.Ocelot" 5000

Write-Host ""
Write-Host "✅ Все сервисы запускаются..." -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Доступные URL:" -ForegroundColor Cyan
Write-Host "  • API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "  • Swagger UI: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "  • Inventory: http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  • Booking: http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  • User: http://localhost:5003/swagger" -ForegroundColor White
Write-Host "  • Payment: http://localhost:5004/swagger" -ForegroundColor White
Write-Host ""
Write-Host "📊 Gateway Stats: http://localhost:5000/api/v1/gateway/stats" -ForegroundColor Magenta
Write-Host ""
Write-Host "⏳ Подождите 30 секунд для полного запуска всех сервисов..." -ForegroundColor Yellow
Write-Host ""

# Открываем браузер через 10 секунд
Start-Sleep -Seconds 10
Write-Host "🌐 Открываем API Gateway в браузере..." -ForegroundColor Green
Start-Process "http://localhost:5000/swagger"

Write-Host ""
Write-Host "🎉 Платформа запущена! Проверьте все окна терминалов." -ForegroundColor Green
Write-Host "========================================================================" -ForegroundColor Cyan


