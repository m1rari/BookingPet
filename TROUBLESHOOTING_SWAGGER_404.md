# 🔧 РЕШЕНИЕ ПРОБЛЕМЫ: 404 /swagger/payments/swagger.json

## ❌ Проблема
```
Fetch error
response status is 404 /swagger/payments/swagger.json
```

## ✅ Причина
API Gateway пытается загрузить Swagger документацию от микросервисов, но они не запущены.

## 🚀 Решение

### Вариант 1: Запустить все сервисы (Рекомендуется)

**Windows (Batch):**
```bash
# Запустите скрипт
start-all-services.bat
```

**Windows (PowerShell):**
```powershell
# Запустите скрипт
.\start-all-services.ps1
```

**Ручной запуск (5 терминалов):**

**Terminal 1 - Inventory Service:**
```bash
cd src/Services/Inventory/Inventory.API
dotnet run --urls http://localhost:5001
```

**Terminal 2 - Booking Service:**
```bash
cd src/Services/Booking/Booking.API
dotnet run --urls http://localhost:5002
```

**Terminal 3 - User Service:**
```bash
cd src/Services/User/User.API
dotnet run --urls http://localhost:5003
```

**Terminal 4 - Payment Service:**
```bash
cd src/Services/Payment/Payment.API
dotnet run --urls http://localhost:5004
```

**Terminal 5 - API Gateway:**
```bash
cd src/ApiGateway/ApiGateway.Ocelot
dotnet run --urls http://localhost:5000
```

### Вариант 2: Запустить только API Gateway (для тестирования)

Если вы хотите протестировать только API Gateway без микросервисов:

```bash
cd src/ApiGateway/ApiGateway.Ocelot
dotnet run --urls http://localhost:5000
```

**Ограничения:**
- Swagger UI будет показывать ошибки 404 для недоступных сервисов
- API вызовы через Gateway будут возвращать ошибки
- Но сам Gateway будет работать и показывать статистику

## 🔍 Проверка статуса

### 1. Проверка микросервисов:
```bash
# Проверьте доступность каждого сервиса
curl http://localhost:5001/swagger/v1/swagger.json  # Inventory
curl http://localhost:5002/swagger/v1/swagger.json  # Booking  
curl http://localhost:5003/swagger/v1/swagger.json  # User
curl http://localhost:5004/swagger/v1/swagger.json  # Payment
```

### 2. Проверка API Gateway:
```bash
# Gateway health
curl http://localhost:5000/health

# Gateway stats (покажет статус всех сервисов)
curl http://localhost:5000/api/v1/gateway/stats
```

### 3. Проверка Swagger UI:
```
http://localhost:5000/swagger
```

## 📊 Ожидаемый результат

После запуска всех сервисов:

✅ **API Gateway Swagger UI** - все сервисы доступны  
✅ **Gateway Stats** - все сервисы показывают "healthy"  
✅ **API Routing** - запросы маршрутизируются корректно  
✅ **JWT Authentication** - работает через Gateway  
✅ **Rate Limiting** - активен  
✅ **Response Caching** - работает  

## 🎯 Быстрый тест

```bash
# 1. Запустите все сервисы (скрипт или вручную)
# 2. Подождите 30 секунд
# 3. Откройте: http://localhost:5000/swagger
# 4. Проверьте: http://localhost:5000/api/v1/gateway/stats
# 5. Тестируйте API через Gateway!
```

## 🚨 Troubleshooting

### Если сервисы не запускаются:

1. **Проверьте Docker контейнеры:**
```bash
docker ps
# Должны быть: postgres-inventory, postgres-booking, postgres-user, rabbitmq, redis
```

2. **Проверьте порты:**
```bash
netstat -an | findstr "5000 5001 5002 5003 5004"
```

3. **Проверьте логи:**
- Каждый сервис выводит логи в своем терминале
- Ищите ошибки подключения к БД или RabbitMQ

### Если Gateway не работает:

1. **Проверьте ocelot.json:**
```bash
# Файл должен существовать
ls src/ApiGateway/ApiGateway.Ocelot/ocelot.json
```

2. **Проверьте Redis:**
```bash
# Redis должен быть доступен
docker exec -it redis redis-cli ping
```

## ✅ ИТОГ

**Для полной функциональности запустите все 5 компонентов:**
- Inventory Service (5001)
- Booking Service (5002)  
- User Service (5003)
- Payment Service (5004)
- API Gateway (5000)

**Используйте скрипты для автоматического запуска!** 🚀


