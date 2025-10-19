# 🚀 Running Guide - Ваша платформа запущена!

## ✅ Инфраструктура работает!

Сейчас запущены **4 контейнера**:

```
✅ postgres-inventory  - PostgreSQL для Inventory Service (порт 5432)
✅ postgres-booking    - PostgreSQL для Booking Service (порт 5433)
✅ rabbitmq           - Message Broker + Management UI (порты 5672, 15672)
✅ redis              - Cache & Distributed Locks (порт 6379)
```

---

## 📋 Следующие шаги

### Шаг 1: Создание миграций EF Core

#### Inventory Service:
```bash
cd src/Services/Inventory/Inventory.Infrastructure

dotnet ef migrations add InitialCreate --startup-project ../Inventory.API/Inventory.API.csproj

dotnet ef database update --startup-project ../Inventory.API/Inventory.API.csproj
```

#### Booking Service:
```bash
cd ../../Booking/Booking.Infrastructure

dotnet ef migrations add InitialCreate --startup-project ../Booking.API/Booking.API.csproj

dotnet ef database update --startup-project ../Booking.API/Booking.API.csproj
```

---

### Шаг 2: Запуск Inventory Service

**Новый терминал:**
```bash
cd src/Services/Inventory/Inventory.API
dotnet run --urls "http://localhost:5001"
```

**Ожидаемый вывод:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Проверка:** http://localhost:5001/swagger

---

### Шаг 3: Запуск Booking Service

**Новый терминал:**
```bash
cd src/Services/Booking/Booking.API
dotnet run --urls "http://localhost:5002"
```

**Проверка:** http://localhost:5002/swagger

---

### Шаг 4: Тестирование Saga Pattern! 🎯

#### Создание ресурса:
```bash
curl -X POST http://localhost:5001/api/v1/resources ^
  -H "Content-Type: application/json" ^
  -d "{\"name\":\"Конференц-зал А\",\"description\":\"Зал на 50 человек\",\"type\":\"ConferenceRoom\",\"address\":\"ул. Ленина, 1\",\"city\":\"Москва\",\"country\":\"Россия\",\"postalCode\":\"123456\",\"maxPeople\":50,\"minPeople\":10,\"pricePerHour\":5000.00}"
```

**Ответ:** `{"id":"guid-здесь"}` - скопируйте этот GUID!

#### Резервация ресурса:
```bash
curl -X POST http://localhost:5001/api/v1/resources/YOUR-GUID-HERE/reserve ^
  -H "Content-Type: application/json" ^
  -d "{\"startTime\":\"2025-10-20T10:00:00Z\",\"endTime\":\"2025-10-20T12:00:00Z\"}"
```

**Ответ:** `{"reservationId":"guid"}`

#### Создание бронирования (Saga!):
```bash
curl -X POST http://localhost:5002/api/v1/bookings ^
  -H "Content-Type: application/json" ^
  -d "{\"resourceId\":\"YOUR-RESOURCE-GUID\",\"userId\":\"00000000-0000-0000-0000-000000000001\",\"startTime\":\"2025-10-21T10:00:00Z\",\"endTime\":\"2025-10-21T12:00:00Z\",\"pricePerHour\":5000.00}"
```

**Это запустит Saga!** 🎯

---

### Шаг 5: Наблюдение за событиями в RabbitMQ

Откройте RabbitMQ Management UI:
**http://localhost:15672** (guest/guest)

Вы увидите:
- ✅ Созданные exchanges
- ✅ Queues для Integration Events
- ✅ Messages в real-time
- ✅ Saga coordination в действии!

Перейдите в **Queues** → увидите события от Saga:
- `ReserveResourceIntegrationEvent`
- `InitiatePaymentIntegrationEvent`

---

## 🔍 Мониторинг

### Health Checks:
```bash
# Inventory Service
curl http://localhost:5001/health

# Booking Service
curl http://localhost:5002/health
```

### Prometheus Metrics (если запущен полный docker-compose):
```bash
# Inventory metrics
curl http://localhost:5001/metrics

# Booking metrics
curl http://localhost:5002/metrics
```

---

## 🧪 Тестовые сценарии

### 1. Happy Path - Успешное бронирование:
```
1. Создать ресурс (POST /resources)
2. Проверить ресурс (GET /resources/{id})
3. Создать бронирование (POST /bookings) ← Saga!
4. Проверить RabbitMQ - увидите события
5. Проверить бронирование (GET /bookings/{id})
```

### 2. Conflict Scenario - Двойное бронирование:
```
1. Создайте бронирование для слота
2. Попробуйте создать еще одно для того же слота
3. Второе получит ошибку благодаря Distributed Lock!
```

### 3. Saga Compensation (когда Payment Service не запущен):
```
1. Создайте бронирование
2. Saga попытается инициировать платеж
3. Если Payment Service недоступен:
   - Автоматический rollback
   - Resource освобождается
   - Booking marked as Failed
```

---

## 📊 Доступные UI

| Сервис | URL | Credentials |
|--------|-----|-------------|
| **RabbitMQ Management** | http://localhost:15672 | guest/guest |
| **Inventory Swagger** | http://localhost:5001/swagger | - |
| **Booking Swagger** | http://localhost:5002/swagger | - |

---

## 🎯 Проверка компонентов

### PostgreSQL:
```bash
# Подключение к Inventory DB
docker exec -it postgres-inventory psql -U postgres -d InventoryDB

# Список таблиц
\dt

# Выход
\q
```

### Redis:
```bash
# Подключение
docker exec -it redis redis-cli

# Проверка ключей
KEYS *

# Выход
exit
```

### RabbitMQ:
```bash
# Список queues
docker exec rabbitmq rabbitmqctl list_queues

# Список exchanges  
docker exec rabbitmq rabbitmqctl list_exchanges
```

---

## 🛠️ Полезные команды

### Перезапуск инфраструктуры:
```bash
docker compose -f docker-compose.minimal.yml down
docker compose -f docker-compose.minimal.yml up -d
```

### Просмотр логов:
```bash
# Все сервисы
docker compose -f docker-compose.minimal.yml logs -f

# Конкретный сервис
docker logs -f rabbitmq
docker logs -f postgres-inventory
```

### Остановка:
```bash
docker compose -f docker-compose.minimal.yml down
```

### Полная очистка (включая volumes):
```bash
docker compose -f docker-compose.minimal.yml down -v
```

---

## 🎊 Поздравляю!

Ваша **Advanced Booking Platform** готова к работе!

### Что дальше:

1. ✅ **Запустите сервисы** (Inventory + Booking)
2. ✅ **Примените миграции** (создайте таблицы)
3. ✅ **Тестируйте API** (создайте ресурсы и бронирования)
4. ✅ **Наблюдайте Saga** (смотрите события в RabbitMQ)
5. ✅ **Экспериментируйте** (тестируйте distributed locks, conflicts)

### Для полного опыта:

Запустите полную инфраструктуру:
```bash
docker compose up -d
```

Это добавит:
- Jaeger (distributed tracing) - http://localhost:16686
- Prometheus (metrics) - http://localhost:9090
- Grafana (dashboards) - http://localhost:3000

---

## 📚 Документация

- **README.md** - общая информация
- **QUICKSTART.md** - этот файл
- **FINAL_REPORT.md** - детальный отчет
- **PROJECT_SUMMARY.md** - краткая сводка
- **ARCHITECTURE.md** - архитектура
- **FILES_CREATED.md** - список файлов

---

**Готово! Наслаждайтесь вашей enterprise-grade платформой!** 🚀

