#!/bin/bash

# Stable Docker Compose Startup Script
# This script starts services step by step to avoid Docker Hub issues

echo "🚀 Starting Booking Platform Infrastructure (Stable Version)"
echo "============================================================="

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

echo "📦 Starting databases..."
docker-compose -f docker-compose.stable.yml up -d postgres-inventory postgres-booking postgres-user sqlserver-payment

echo "⏳ Waiting for databases to be ready..."
sleep 20

echo "📦 Starting NoSQL databases..."
docker-compose -f docker-compose.stable.yml up -d mongodb-review mongodb-analytics

echo "📦 Starting message broker..."
docker-compose -f docker-compose.stable.yml up -d rabbitmq

echo "📦 Starting cache..."
docker-compose -f docker-compose.stable.yml up -d redis

echo "⏳ Waiting for core services..."
sleep 15

echo "📦 Starting search engine..."
docker-compose -f docker-compose.stable.yml up -d elasticsearch

echo "📦 Starting service discovery..."
docker-compose -f docker-compose.stable.yml up -d consul

echo "⏳ Waiting for infrastructure services..."
sleep 10

echo "📦 Starting observability stack..."
docker-compose -f docker-compose.stable.yml up -d jaeger prometheus grafana

echo "⏳ Waiting for observability services..."
sleep 15

echo "🔐 Starting IdentityServer..."
docker-compose -f docker-compose.stable.yml up -d identityserver

echo "⏳ Waiting for IdentityServer to be ready..."
sleep 15

echo ""
echo "✅ Infrastructure services started successfully!"
echo "================================================================"
echo "🔐 IdentityServer:     https://localhost:5005"
echo "📊 Monitoring:"
echo "   - Jaeger UI:        http://localhost:16686"
echo "   - Prometheus:       http://localhost:9090"
echo "   - Grafana:          http://localhost:3000 (admin/admin)"
echo "   - RabbitMQ UI:      http://localhost:15672 (guest/guest)"
echo ""
echo "🌐 Now you can start the .NET services manually:"
echo "   - API Gateway:      dotnet run --project src/ApiGateway/ApiGateway.Ocelot"
echo "   - User Service:     dotnet run --project src/Services/User/User.API"
echo "   - Booking Service:  dotnet run --project src/Services/Booking/Booking.API"
echo "   - Inventory Service: dotnet run --project src/Services/Inventory/Inventory.API"
echo "   - Payment Service:  dotnet run --project src/Services/Payment/Payment.API"
echo ""
echo "🛑 To stop infrastructure: docker-compose -f docker-compose.stable.yml down"
echo ""

# Keep script running
echo "🔄 Infrastructure is running. Press Ctrl+C to stop."
wait
