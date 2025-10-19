#!/bin/bash

# Dual Authentication Architecture Startup Script
# This script starts all services with the new two-level authentication system

echo "🚀 Starting Booking Platform with Dual Authentication Architecture"
echo "================================================================"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

echo "📦 Starting infrastructure services..."
docker-compose up -d postgres-inventory postgres-booking postgres-user sqlserver-payment mongodb-review mongodb-analytics rabbitmq redis elasticsearch consul jaeger prometheus grafana

echo "⏳ Waiting for infrastructure services to be ready..."
sleep 30

echo "🔐 Starting IdentityServer..."
docker-compose up -d identityserver

echo "⏳ Waiting for IdentityServer to be ready..."
sleep 15

echo "🌐 Starting API Gateway..."
cd src/ApiGateway/ApiGateway.Ocelot
dotnet run --urls="https://localhost:5000" &
GATEWAY_PID=$!
cd ../../..

echo "👤 Starting User Service..."
cd src/Services/User/User.API
dotnet run --urls="https://localhost:5004" &
USER_PID=$!
cd ../../..

echo "📅 Starting Booking Service..."
cd src/Services/Booking/Booking.API
dotnet run --urls="https://localhost:5001" &
BOOKING_PID=$!
cd ../../..

echo "📦 Starting Inventory Service..."
cd src/Services/Inventory/Inventory.API
dotnet run --urls="https://localhost:5002" &
INVENTORY_PID=$!
cd ../../..

echo "💳 Starting Payment Service..."
cd src/Services/Payment/Payment.API
dotnet run --urls="https://localhost:5003" &
PAYMENT_PID=$!
cd ../../..

echo "⏳ Waiting for all services to start..."
sleep 20

echo ""
echo "✅ All services started successfully!"
echo "================================================================"
echo "🔐 IdentityServer:     https://localhost:5005"
echo "🌐 API Gateway:        https://localhost:5000"
echo "👤 User Service:       https://localhost:5004"
echo "📅 Booking Service:    https://localhost:5001"
echo "📦 Inventory Service:  https://localhost:5002"
echo "💳 Payment Service:    https://localhost:5003"
echo ""
echo "📊 Monitoring:"
echo "   - Jaeger UI:        http://localhost:16686"
echo "   - Prometheus:       http://localhost:9090"
echo "   - Grafana:          http://localhost:3000 (admin/admin)"
echo "   - RabbitMQ UI:      http://localhost:15672 (guest/guest)"
echo ""
echo "🧪 Test the dual authentication:"
echo "   1. Use 'dual-auth-tests.http' file for testing"
echo "   2. First get service token from IdentityServer"
echo "   3. Then test user authentication through Gateway"
echo "   4. Finally test service-to-service communication"
echo ""
echo "🛑 To stop all services:"
echo "   - Press Ctrl+C to stop this script"
echo "   - Run: docker-compose down"
echo ""

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "🛑 Stopping services..."
    kill $GATEWAY_PID $USER_PID $BOOKING_PID $INVENTORY_PID $PAYMENT_PID 2>/dev/null
    docker-compose down
    echo "✅ All services stopped."
    exit 0
}

# Set trap to cleanup on script exit
trap cleanup SIGINT SIGTERM

# Keep script running
echo "🔄 Services are running. Press Ctrl+C to stop all services."
wait
