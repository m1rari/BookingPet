@echo off
echo ========================================================================
echo  🚀 BOOKING PLATFORM - QUICK START SCRIPT
echo ========================================================================
echo.

echo 📋 Запуск всех микросервисов и API Gateway...
echo.

echo 🔧 Запуск Inventory Service (Port 5001)...
start "Inventory Service" cmd /k "cd src\Services\Inventory\Inventory.API && dotnet run --urls http://localhost:5001"

echo 🔧 Запуск Booking Service (Port 5002)...
start "Booking Service" cmd /k "cd src\Services\Booking\Booking.API && dotnet run --urls http://localhost:5002"

echo 🔧 Запуск User Service (Port 5003)...
start "User Service" cmd /k "cd src\Services\User\User.API && dotnet run --urls http://localhost:5003"

echo 🔧 Запуск Payment Service (Port 5004)...
start "Payment Service" cmd /k "cd src\Services\Payment\Payment.API && dotnet run --urls http://localhost:5004"

echo 🔧 Запуск API Gateway (Port 5000)...
start "API Gateway" cmd /k "cd src\ApiGateway\ApiGateway.Ocelot && dotnet run --urls http://localhost:5000"

echo.
echo ✅ Все сервисы запускаются...
echo.
echo 🌐 Доступные URL:
echo   • API Gateway: http://localhost:5000
echo   • Swagger UI: http://localhost:5000/swagger
echo   • Inventory: http://localhost:5001/swagger
echo   • Booking: http://localhost:5002/swagger
echo   • User: http://localhost:5003/swagger
echo   • Payment: http://localhost:5004/swagger
echo.
echo 📊 Gateway Stats: http://localhost:5000/api/v1/gateway/stats
echo.
echo ⏳ Подождите 30 секунд для полного запуска всех сервисов...
echo.
pause


