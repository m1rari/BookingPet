# 🚀 Руководство по запуску системы с двухуровневой аутентификацией

## ✅ Проблема решена!

Мы успешно решили проблему с Docker Hub и создали рабочую систему двухуровневой аутентификации.

## 🏗️ Что было исправлено:

### 1. Проблемы с Entity Framework миграциями
- **Проблема**: EF не мог создать DbContext из-за зависимостей от MediatR и других сервисов
- **Решение**: Создали DesignTime Factory для каждого DbContext
- **Файлы**: 
  - `UserDbContextDesignTimeFactory.cs`
  - `BookingDbContextDesignTimeFactory.cs` 
  - `InventoryDbContextDesignTimeFactory.cs`

### 2. Проблемы с Docker Hub
- **Проблема**: Ошибка 403 Forbidden при загрузке образов
- **Решение**: Создали стабильную версию docker-compose с Alpine образами
- **Файлы**: `docker-compose.stable.yml`

### 3. Проблемы с MockMediator
- **Проблема**: Сложности с реализацией интерфейса MediatR
- **Решение**: Упростили подход - используем null mediator для design-time

## 🚀 Способы запуска системы:

### Вариант 1: Полный запуск через Docker (рекомендуется)
```powershell
# Запуск только инфраструктуры (стабильные образы)
docker-compose -f docker-compose.stable.yml up -d postgres-inventory postgres-booking postgres-user redis

# Запуск .NET сервисов
.\start-dotnet-services.ps1
```

### Вариант 2: Пошаговый запуск
```powershell
# 1. Запуск инфраструктуры
docker-compose -f docker-compose.stable.yml up -d postgres-inventory postgres-booking postgres-user redis

# 2. Запуск IdentityServer
cd src/IdentityServer
dotnet run --urls="https://localhost:5005"

# 3. В новых терминалах запустить остальные сервисы
cd src/ApiGateway/ApiGateway.Ocelot
dotnet run --urls="https://localhost:5000"

cd src/Services/User/User.API
dotnet run --urls="https://localhost:5004"

cd src/Services/Booking/Booking.API
dotnet run --urls="https://localhost:5001"

cd src/Services/Inventory/Inventory.API
dotnet run --urls="https://localhost:5002"

cd src/Services/Payment/Payment.API
dotnet run --urls="https://localhost:5003"
```

## 🧪 Тестирование системы:

### 1. Получение Client Credentials токена
```http
POST https://localhost:5005/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&client_id=api-gateway&client_secret=gateway-secret-key&scope=inventory.api bookings.api users.api payments.api
```

### 2. Пользовательская аутентификация
```http
POST https://localhost:5000/api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password"
}
```

### 3. Тестирование защищенных endpoints
```http
GET https://localhost:5001/api/v1/bookings
Authorization: Bearer <service-token>
```

## 📊 Архитектура системы:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Пользователь   │───▶│   API Gateway   │───▶│  Микросервисы   │
│                 │    │                 │    │                 │
│ JWT Token       │    │ User JWT Check  │    │ Client Creds    │
│ (User Auth)     │    │ + Service Token │    │ (Service Auth)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │ IdentityServer  │
                       │                 │
                       │ Client Creds    │
                       │ Token Issuer    │
                       └─────────────────┘
```

## 🔐 Безопасность:

- **Defence in Depth** - Многоуровневая защита
- **Изоляция сервисов** - Прямой доступ невозможен без Client Credentials
- **Гранулярные права** - Разные scope для разных сервисов
- **Кэширование токенов** - Оптимизация производительности

## 📁 Созданные файлы:

### DesignTime Factories:
- `src/Services/User/User.Infrastructure/Persistence/UserDbContextDesignTimeFactory.cs`
- `src/Services/Booking/Booking.Infrastructure/Persistence/BookingDbContextDesignTimeFactory.cs`
- `src/Services/Inventory/Inventory.Infrastructure/Persistence/InventoryDbContextDesignTimeFactory.cs`

### Docker конфигурации:
- `docker-compose.stable.yml` - Стабильные образы
- `start-infrastructure-stable.ps1` - Скрипт запуска инфраструктуры

### Скрипты запуска:
- `start-dotnet-services.ps1` - Запуск всех .NET сервисов
- `dual-auth-tests.http` - HTTP тесты

## ✅ Результат:

Система готова к использованию! Все проблемы решены:

1. ✅ Entity Framework миграции работают
2. ✅ Docker образы загружаются стабильно  
3. ✅ Двухуровневая аутентификация реализована
4. ✅ Все сервисы могут быть запущены
5. ✅ Тесты готовы для проверки

## 🎯 Следующие шаги:

1. Запустите систему используя `start-dotnet-services.ps1`
2. Протестируйте аутентификацию через `dual-auth-tests.http`
3. Настройте мониторинг и логирование
4. Подготовьте к production развертыванию
