# 🚀 Quick Start Guide - Advanced Booking Platform

## Быстрый запуск за 5 минут!

### Шаг 1: Запуск инфраструктуры (1 команда!)

**Вариант 1 - Минимальный (быстрый старт):**
```bash
docker compose -f docker-compose.minimal.yml up -d
```

Это запустит **4 основных сервиса**:
- ✅ PostgreSQL для Inventory (порт 5432)
- ✅ PostgreSQL для Booking (порт 5433)
- ✅ RabbitMQ (message broker + UI)
- ✅ Redis (cache & distributed locks)

**Вариант 2 - Полный (все 11 сервисов):**
```bash
docker compose up -d
```

Добавляет: PostgreSQL для User, SQL Server, MongoDB × 2, Elasticsearch, Consul, Jaeger, Prometheus, Grafana

**Проверка запущенных контейнеров:**
```bash
docker ps
```

**RabbitMQ Management UI:** http://localhost:15672 (guest/guest)

---

### Шаг 2: Применение миграций БД

```bash
# Inventory DB
cd src/Services/Inventory/Inventory.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Inventory.API
dotnet ef database update --startup-project ../Inventory.API

# Booking DB
cd ../../Booking/Booking.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Booking.API
dotnet ef database update --startup-project ../Booking.API
```

---

### Шаг 3: Запуск сервисов

**Терминал 1 - Inventory Service:**
```bash
cd src/Services/Inventory/Inventory.API
dotnet run --urls "http://localhost:5001"
```

**Терминал 2 - Booking Service:**
```bash
cd src/Services/Booking/Booking.API
dotnet run --urls "http://localhost:5002"
```

**Терминал 3 - API Gateway:**
```bash
cd src/ApiGateway/ApiGateway.Ocelot
dotnet run --urls "http://localhost:5000"
```

---

### Шаг 4: Тестирование!

#### 1. Откройте Swagger UI:
http://localhost:5000/swagger

#### 2. Создайте ресурс:
```bash
POST http://localhost:5000/api/v1/resources
{
  "name": "Конференц-зал А",
  "description": "Зал на 50 человек",
  "type": "ConferenceRoom",
  "address": "ул. Ленина, 1",
  "city": "Москва",
  "country": "Россия",
  "postalCode": "123456",
  "maxPeople": 50,
  "minPeople": 10,
  "pricePerHour": 5000.00
}
```

#### 3. Создайте бронирование (Saga!):
```bash
POST http://localhost:5000/api/v1/bookings
{
  "resourceId": "guid-from-step-2",
  "userId": "00000000-0000-0000-0000-000000000001",
  "startTime": "2025-10-20T10:00:00Z",
  "endTime": "2025-10-20T12:00:00Z",
  "pricePerHour": 5000.00
}
```

---

### Шаг 5: Мониторинг

#### Jaeger (Distributed Tracing):
http://localhost:16686
- Найдите traces для вашего бронирования
- Увидите весь flow: API Gateway → Booking → Inventory

#### Prometheus (Metrics):
http://localhost:9090
- Запросы: `http_requests_total`
- Latency: `http_request_duration_seconds`

#### Grafana (Dashboards):
http://localhost:3000 (admin/admin)
- Импортируйте dashboards для визуализации

#### RabbitMQ Management:
http://localhost:15672 (guest/guest)
- Смотрите Integration Events в real-time

---

## 🎯 Что тестировать

### 1. Distributed Locking:
Попробуйте резервировать один и тот же ресурс одновременно из двух браузеров - второй запрос получит ошибку conflict!

### 2. Saga Pattern:
Создайте бронирование и наблюдайте в RabbitMQ как публикуются Integration Events для координации между сервисами.

### 3. Circuit Breaker:
Остановите Payment Service и создайте бронирование - увидите graceful degradation!

### 4. Distributed Tracing:
Откройте Jaeger и найдите trace для запроса - увидите весь путь через микросервисы!

### 5. Rate Limiting:
Сделайте 101 запрос за минуту - получите HTTP 429 (Too Many Requests)!

---

## 📊 Порты сервисов

| Сервис | Порт | URL |
|--------|------|-----|
| **API Gateway** | 5000 | http://localhost:5000 |
| Inventory Service | 5001 | http://localhost:5001 |
| Booking Service | 5002 | http://localhost:5002 |
| User Service | 5003 | http://localhost:5003 |
| Payment Service | 5004 | http://localhost:5004 |
| Review Service | 5005 | http://localhost:5005 |
| Analytics Service | 5006 | http://localhost:5006 |
| **RabbitMQ UI** | 15672 | http://localhost:15672 |
| **Jaeger UI** | 16686 | http://localhost:16686 |
| **Prometheus** | 9090 | http://localhost:9090 |
| **Grafana** | 3000 | http://localhost:3000 |
| **Consul** | 8500 | http://localhost:8500 |
| **Elasticsearch** | 9200 | http://localhost:9200 |

---

## 🐛 Troubleshooting

### Проблема: Порт уже используется
```bash
# Найти процесс
netstat -ano | findstr :5001
# Убить процесс
taskkill /PID <pid> /F
```

### Проблема: Docker контейнеры не запускаются
```bash
docker-compose down
docker-compose up -d --force-recreate
```

### Проблема: Миграции не применяются
```bash
# Удалите БД и создайте заново
docker-compose down -v
docker-compose up -d
```

---

## 🎓 Обучение

Этот проект идеально подходит для изучения:
- Микросервисной архитектуры
- DDD и Clean Architecture
- Saga Pattern
- Circuit Breaker
- Event-Driven Design
- Docker/Kubernetes
- Observability

**Исследуйте код, экспериментируйте, учитесь!** 📚

---

**Готово! Наслаждайтесь вашей enterprise-grade платформой бронирования!** 🎉


