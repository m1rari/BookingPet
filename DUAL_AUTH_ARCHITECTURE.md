# Двухуровневая аутентификация в микросервисной архитектуре

## Обзор архитектуры

Реализована двухуровневая система аутентификации для микросервисной платформы бронирования:

1. **Пользовательская аутентификация** - JWT токены для пользователей через API Gateway
2. **Межсервисная аутентификация** - Client Credentials токены для защиты взаимодействия между сервисами

## Компоненты системы

### 1. IdentityServer (Duende)
- **Порт**: 5005 (HTTPS)
- **Роль**: Выдача Client Credentials токенов для межсервисного взаимодействия
- **Конфигурация**: `src/IdentityServer/Config.cs`

#### API Scopes:
- `inventory.api` - Inventory Service
- `bookings.api` - Booking Service  
- `users.api` - User Service
- `payments.api` - Payment Service
- `reviews.api` - Review Service
- `analytics.api` - Analytics Service

#### Clients:
- `api-gateway` - Gateway клиент (доступ ко всем сервисам)
- `booking-service` - Booking Service клиент
- `payment-service` - Payment Service клиент
- `review-service` - Review Service клиент
- `analytics-service` - Analytics Service клиент

### 2. API Gateway
- **Порт**: 5000 (HTTPS)
- **Роль**: 
  - Проверка пользовательских JWT токенов
  - Получение Client Credentials токенов от IdentityServer
  - Проксирование запросов с добавлением служебных токенов

#### Компоненты:
- `TokenService` - Кэширование и получение Client Credentials токенов
- `ServiceTokenDelegatingHandler` - Автоматическое добавление токенов к исходящим запросам

### 3. Микросервисы
Все микросервисы защищены Client Credentials аутентификацией:

#### Booking Service (5001)
- **Scope**: `bookings.api`
- **Политика**: `ApiScope`

#### Inventory Service (5002)  
- **Scope**: `inventory.api`
- **Политика**: `ApiScope`

#### Payment Service (5003)
- **Scope**: `payments.api`
- **Политика**: `ApiScope`

#### User Service (5004)
- **Двойная роль**:
  - Выдача пользовательских JWT токенов через `/login` (без Client Credentials)
  - Защита остальных endpoints через Client Credentials (`users.api` scope)

## Поток аутентификации

### 1. Пользовательская аутентификация
```
Пользователь → API Gateway → User Service (/login)
                ↓
            JWT токен пользователя
                ↓
Пользователь → API Gateway (с JWT) → Микросервис (с Client Credentials)
```

### 2. Межсервисное взаимодействие
```
API Gateway → IdentityServer (Client Credentials)
                ↓
            Service Token
                ↓
API Gateway → Микросервис (с Service Token)
```

## Конфигурация

### IdentityServer
```json
{
  "IdentityServer": {
    "Authority": "https://localhost:5005",
    "ClientId": "api-gateway",
    "ClientSecret": "gateway-secret-key"
  }
}
```

### Микросервисы
```json
{
  "IdentityServer": {
    "Authority": "https://localhost:5005"
  }
}
```

## Безопасность

### Преимущества архитектуры:
1. **Defence in Depth** - Многоуровневая защита
2. **Изоляция сервисов** - Прямой доступ к микросервисам невозможен без Client Credentials
3. **Кэширование токенов** - Оптимизация производительности
4. **Гранулярные права** - Разные scope для разных сервисов

### Важные моменты:
- **HTTPS в Production** - Обязательно использовать `RequireHttpsMetadata = true`
- **Безопасное хранение секретов** - Использовать Azure Key Vault, AWS Secrets Manager или Kubernetes Secrets
- **Health checks** - Endpoint `/health` доступен без аутентификации
- **Токены пользователей** - Проверяются только на Gateway
- **Служебные токены** - Проверяются микросервисами

## Тестирование

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

## Запуск системы

### Windows (PowerShell)
```powershell
.\start-dual-auth.ps1
```

### Linux/macOS (Bash)
```bash
chmod +x start-dual-auth.sh
./start-dual-auth.sh
```

## Мониторинг

- **Jaeger UI**: http://localhost:16686
- **Prometheus**: http://localhost:9090  
- **Grafana**: http://localhost:3000 (admin/admin)
- **RabbitMQ UI**: http://localhost:15672 (guest/guest)

## Troubleshooting

### Проблемы с токенами
1. Проверить доступность IdentityServer
2. Проверить правильность ClientId/ClientSecret
3. Проверить scope в токене
4. Проверить срок действия токена

### Проблемы с Gateway
1. Проверить конфигурацию Ocelot
2. Проверить DelegatingHandler
3. Проверить логи TokenService

### Проблемы с микросервисами
1. Проверить конфигурацию IdentityServer Authority
2. Проверить политики авторизации
3. Проверить middleware pipeline
