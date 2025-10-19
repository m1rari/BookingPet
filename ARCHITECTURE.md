# 🏛️ Advanced Booking Platform - Architecture Overview

## 🎯 Системная архитектура

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              CLIENT                                      │
│                      (Web, Mobile, Desktop App)                          │
└────────────────────────────┬────────────────────────────────────────────┘
                             │
                             │ HTTP/REST
                             ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY (Ocelot)                             │
│  • Routing • Rate Limiting • Load Balancing • Authentication            │
│                      http://localhost:5000                               │
└──┬────┬────┬────┬────┬────┬────────────────────────────────────────────┘
   │    │    │    │    │    │
   │    │    │    │    │    │        MICROSERVICES LAYER
   ▼    ▼    ▼    ▼    ▼    ▼
┌─────┐┌────┐┌────┐┌────┐┌────┐┌─────┐
│Invt ││Book││User││Pay ││Rev ││Anlt │
│:5001││:5002│:5003││:5004│:5005││:5006│
│     ││    ││    ││    ││    ││     │
│PG   ││PG  ││PG  ││SQL ││Mng ││Mng  │
│Redis││    ││    ││Srv ││DB  ││DB   │
│     ││    ││    ││    ││    ││ES   │
└──┬──┘└─┬──┘└─┬──┘└─┬──┘└─┬──┘└──┬──┘
   │     │     │     │     │     │
   └─────┴─────┴─────┴─────┴─────┘
                  │
                  │ Integration Events
                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                       EVENT BUS (RabbitMQ)                               │
│  • Pub/Sub • Saga Coordination • Async Communication                    │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│                    CROSS-CUTTING CONCERNS                                │
│  Redis (Cache & Locks) • Consul (Discovery) • Jaeger (Tracing)         │
│  Prometheus (Metrics) • Grafana (Dashboards) • Elasticsearch (Search)  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🎨 Clean Architecture (каждый сервис)

```
┌──────────────────────────────────────────┐
│              API Layer                   │
│  Controllers • Middleware • Program.cs   │
│  Swagger • Health Checks                 │
└────────────┬─────────────────────────────┘
             │ depends on
             ▼
┌──────────────────────────────────────────┐
│        Infrastructure Layer              │
│  EF Core • Repositories • External APIs  │
│  MassTransit • Redis • Polly             │
└────────────┬─────────────────────────────┘
             │ depends on
             ▼
┌──────────────────────────────────────────┐
│        Application Layer                 │
│  Commands • Queries • Handlers • Sagas   │
│  DTOs • Interfaces • Business Logic      │
└────────────┬─────────────────────────────┘
             │ depends on
             ▼
┌──────────────────────────────────────────┐
│          Domain Layer                    │
│  Aggregates • Entities • Value Objects   │
│  Domain Events • Business Rules          │
│  (NO DEPENDENCIES!)                      │
└──────────────────────────────────────────┘
```

---

## 🔄 Saga Pattern Flow

### CreateBookingSaga (Booking Service):

```
┌─────────────┐
│   CLIENT    │
└──────┬──────┘
       │ POST /bookings
       ▼
┌─────────────────────────────────────────┐
│    Booking Service (Saga Orchestrator)  │
└──────┬──────────────┬───────────────────┘
       │              │
       │ Step 1       │ Step 3 (on success)
       │              │
       ▼              ▼
┌─────────────┐  ┌─────────────┐
│  Inventory  │  │   Payment   │
│   Service   │  │   Service   │
│             │  │             │
│ Reserve     │  │ Initiate    │
│ Resource    │  │ Payment     │
└──────┬──────┘  └──────┬──────┘
       │                │
       │ Success        │ Success
       ▼                ▼
┌─────────────────────────────────────────┐
│      Booking Service                    │
│      ConfirmBooking()                   │
│      Status: CONFIRMED ✅                │
└─────────────────────────────────────────┘

        ON FAILURE - COMPENSATE:
        ┌────────────────┐
        │ Release        │
        │ Resource       │
        └────────────────┘
        ┌────────────────┐
        │ Cancel         │
        │ Payment        │
        └────────────────┘
        ┌────────────────┐
        │ Mark Booking   │
        │ as FAILED      │
        └────────────────┘
```

---

## 🛡️ Resilience Patterns

### Circuit Breaker (Payment Service):

```
STATE MACHINE:

    CLOSED (Normal)
        │
        │ 3 consecutive failures
        ▼
    OPEN (Block requests)
        │
        │ Wait 30 seconds
        ▼
    HALF-OPEN (Test)
        │
        ├─ Success → CLOSED
        └─ Failure → OPEN

Benefits:
✅ Fail-fast (no waiting)
✅ System recovery time
✅ Prevents cascade failures
```

---

## 🔐 Security Architecture

```
┌────────────┐
│   CLIENT   │
└─────┬──────┘
      │ 1. Login request
      ▼
┌──────────────────┐
│  User Service    │
│  (Identity)      │
└─────┬────────────┘
      │ 2. Generate JWT
      │ (BuildingBlocks.Authentication)
      ▼
┌────────────┐
│   CLIENT   │
│  (stores   │
│   token)   │
└─────┬──────┘
      │ 3. Request with JWT
      │ Authorization: Bearer <token>
      ▼
┌──────────────────┐
│  API Gateway     │
│  (validates JWT) │
└─────┬────────────┘
      │ 4. Authenticated request
      ▼
┌──────────────────┐
│  Microservice    │
│  (authorized)    │
└──────────────────┘
```

---

## 📊 Data Flow

### Create Booking Flow:

```
1. CLIENT
   │
   ▼
2. API Gateway (:5000)
   │ Rate Limit Check
   │ Route to Booking
   ▼
3. Booking Service (:5002)
   │ CreateBookingSaga.Execute()
   │
   ├─► 4. Publish: ReserveResourceEvent
   │      │
   │      ▼
   │   Inventory Service (:5001)
   │      │ Acquire Redis Lock
   │      │ Reserve Slot
   │      │ Publish: ResourceReservedEvent
   │      ▼
   │   [RabbitMQ Queue]
   │
   └─► 5. Publish: InitiatePaymentEvent
          │
          ▼
       Payment Service (:5004)
          │ Circuit Breaker
          │ Call External Gateway
          │ Publish: PaymentCompletedEvent
          ▼
       [RabbitMQ Queue]
          │
          ▼
6. Booking Service (Event Consumer)
   │ Receive both events
   │ Confirm Booking
   ▼
7. Status: CONFIRMED ✅

   ALL TRACED IN JAEGER! 🔍
```

---

## 🗄️ Database Architecture

```
Inventory Service
├─ PostgreSQL (Port 5432)
   ├─ Resources table
   └─ TimeSlots table

Booking Service
├─ PostgreSQL (Port 5433)
   └─ Bookings table (with RowVersion)

User Service
├─ PostgreSQL (Port 5434)
   ├─ AspNetUsers
   ├─ AspNetRoles
   └─ AspNetUserRoles

Payment Service
├─ SQL Server (Port 1433)
   └─ Payments table

Review Service
├─ MongoDB (Port 27017)
   └─ reviews collection

Analytics Service
├─ MongoDB (Port 27018)
   └─ analytics collection
└─ Elasticsearch (Port 9200)
   └─ resources index
```

---

## 🌐 Network Topology

```
Docker Network: booking-platform

┌────────────────────────────────────────────────┐
│                                                │
│  Microservices                                 │
│  ├─ inventory-service                          │
│  ├─ booking-service                            │
│  ├─ user-service                               │
│  ├─ payment-service                            │
│  ├─ review-service                             │
│  └─ analytics-service                          │
│                                                │
│  Databases                                     │
│  ├─ postgres-inventory                         │
│  ├─ postgres-booking                           │
│  ├─ postgres-user                              │
│  ├─ sqlserver-payment                          │
│  ├─ mongodb-review                             │
│  └─ mongodb-analytics                          │
│                                                │
│  Infrastructure                                │
│  ├─ rabbitmq                                   │
│  ├─ redis                                      │
│  ├─ elasticsearch                              │
│  ├─ consul                                     │
│  ├─ jaeger                                     │
│  ├─ prometheus                                 │
│  └─ grafana                                    │
│                                                │
│  API Gateway                                   │
│  └─ ocelot-gateway                             │
│                                                │
└────────────────────────────────────────────────┘

All services communicate within this network!
```

---

## 🎯 Design Decisions

### Why Microservices?
- ✅ Independent deployment
- ✅ Technology diversity (PostgreSQL, MongoDB, SQL Server)
- ✅ Team autonomy
- ✅ Fault isolation

### Why Event-Driven?
- ✅ Loose coupling
- ✅ Scalability
- ✅ Async processing
- ✅ Event sourcing ready

### Why Saga?
- ✅ Distributed transactions across services
- ✅ Eventual consistency
- ✅ Automatic compensation
- ✅ Business process orchestration

### Why Circuit Breaker?
- ✅ Fail-fast
- ✅ System stability
- ✅ Graceful degradation
- ✅ Recovery time

### Why Clean Architecture?
- ✅ Testability
- ✅ Maintainability
- ✅ Technology agnostic domain
- ✅ Clear separation of concerns

---

## 📦 Deployment Options

### Local Development:
```bash
docker-compose up -d
dotnet run (each service)
```

### Docker (All services):
```bash
# Build images
docker build -t bookingplatform/inventory:1.0 -f src/Services/Inventory/Inventory.API/Dockerfile .
# ... (for each service)

# Run with docker-compose
docker-compose -f docker-compose.prod.yml up -d
```

### Kubernetes:
```bash
# Apply manifests
kubectl apply -f k8s/

# Check status
kubectl get pods
kubectl get services
kubectl get hpa
```

### Cloud (Azure/AWS/GCP):
- Azure Kubernetes Service (AKS)
- Amazon Elastic Kubernetes Service (EKS)
- Google Kubernetes Engine (GKE)

---

## 🎊 ФИНАЛ

**Advanced Booking Platform** - это:

✅ **Production-Ready** микросервисная платформа  
✅ **30 проектов** в solution  
✅ **6 микросервисов** с полной реализацией  
✅ **25+ enterprise паттернов**  
✅ **Saga Pattern** для distributed transactions  
✅ **Circuit Breaker** для resilience  
✅ **Full Infrastructure** (Docker, K8s, Observability)  

**Готово к использованию, обучению и расширению!** 🚀

---

**См. также:**
- `README.md` - общая информация
- `QUICKSTART.md` - быстрый старт
- `FINAL_REPORT.md` - детальный отчет
- `PROJECT_SUMMARY.md` - краткая сводка


