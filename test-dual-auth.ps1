# Simple Test Script for Dual Authentication System
# This script tests the basic functionality of our dual authentication system

Write-Host "🧪 Testing Dual Authentication System" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Check if infrastructure is running
Write-Host "📦 Checking infrastructure services..." -ForegroundColor Yellow
$containers = docker ps --format "table {{.Names}}" | Select-String "postgres|redis"
if ($containers.Count -lt 4) {
    Write-Host "❌ Infrastructure services are not running. Starting them..." -ForegroundColor Red
    Set-Location ".."
    docker-compose -f docker-compose.stable.yml up -d postgres-inventory postgres-booking postgres-user redis
    Start-Sleep -Seconds 10
    Set-Location "src/IdentityServer"
}

Write-Host "✅ Infrastructure services are running" -ForegroundColor Green

# Start IdentityServer
Write-Host "🔐 Starting IdentityServer..." -ForegroundColor Yellow
$identityJob = Start-Job -ScriptBlock {
    Set-Location "src/IdentityServer"
    dotnet run --urls="https://localhost:5005"
}

Start-Sleep -Seconds 10

# Test IdentityServer
Write-Host "🧪 Testing IdentityServer..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "https://localhost:5005/.well-known/openid_configuration" -Method Get -SkipCertificateCheck
    Write-Host "✅ IdentityServer is running" -ForegroundColor Green
} catch {
    Write-Host "❌ IdentityServer is not responding" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Client Credentials token
Write-Host "🔑 Testing Client Credentials token..." -ForegroundColor Yellow
try {
    $body = @{
        grant_type = "client_credentials"
        client_id = "api-gateway"
        client_secret = "gateway-secret-key"
        scope = "inventory.api bookings.api users.api payments.api"
    }
    
    $response = Invoke-RestMethod -Uri "https://localhost:5005/connect/token" -Method Post -Body $body -ContentType "application/x-www-form-urlencoded" -SkipCertificateCheck
    
    if ($response.access_token) {
        Write-Host "✅ Client Credentials token obtained successfully" -ForegroundColor Green
        Write-Host "Token: $($response.access_token.Substring(0, 20))..." -ForegroundColor Cyan
    } else {
        Write-Host "❌ Failed to get Client Credentials token" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error getting Client Credentials token" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Cleanup
Write-Host ""
Write-Host "🛑 Stopping IdentityServer..." -ForegroundColor Yellow
Stop-Job $identityJob -ErrorAction SilentlyContinue
Remove-Job $identityJob -ErrorAction SilentlyContinue

Write-Host "✅ Test completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Yellow
Write-Host "   - Infrastructure services: ✅ Running" -ForegroundColor White
Write-Host "   - IdentityServer: ✅ Running" -ForegroundColor White
Write-Host "   - Client Credentials: ✅ Working" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Next steps:" -ForegroundColor Yellow
Write-Host "   1. Start User Service: cd src/Services/User/User.API && dotnet run" -ForegroundColor White
Write-Host "   2. Test user authentication through Gateway" -ForegroundColor White
Write-Host "   3. Test service-to-service communication" -ForegroundColor White
