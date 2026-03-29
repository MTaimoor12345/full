# SportsStore - Distributed Order Processing Platform

A comprehensive distributed order processing platform built with .NET 10 microservices architecture, featuring RabbitMQ messaging, CQRS pattern, Blazor WebAssembly customer portal, and React admin dashboard.



## Technology Stack

### Backend
- .NET 10 - Web API microservices
- Entity Framework Core with SQLite
- MediatR - CQRS implementation
- MassTransit - RabbitMQ messaging
- AutoMapper - Object mapping
- Serilog - Structured logging

### Frontend
- Blazor WebAssembly - Customer portal
- React 18 + Vite - Admin dashboard (JavaScript)
- Bootstrap 5 - UI styling

### Infrastructure
- RabbitMQ - Message broker
- Docker - Containerization
- GitHub Actions - CI/CD pipeline

## Project Structure

```
SportsStore/
├── SportsStore.Shared/           
├── SportsStore.OrderAPI/         
├── SportsStore.InventoryService/
├── SportsStore.PaymentService/  
├── SportsStore.ShippingService/  
├── SportsStore.Blazor/           
├── SportsStore.Admin/           
├── SportsStore.Tests/           
├── SportsStore.IntegrationTests/ 
├── SportsStore.OrderAPI.Tests/   
├── SportsStore.InventoryService.Tests/
├── SportsStore.PaymentService.Tests/
├── SportsStore.ShippingService.Tests/
├── docker-compose.yml            
├── .dockerignore                
└── .github/workflows/ci.yml     
```

## Microservices

### 1. OrderAPI (Port 5000)
The main gateway service handling order management with CQRS pattern.

**Commands:**
- `CheckoutOrderCommand` - Process new orders
- `CancelOrderCommand` - Cancel existing orders
- `ProcessInventoryResultCommand` - Handle inventory results
- `ProcessPaymentResultCommand` - Handle payment results
- `CreateShipmentCommand` - Create shipments

**Queries:**
- `GetOrderByIdQuery` - Get order details
- `GetOrdersQuery` - Paginated order list
- `GetCustomerOrdersQuery` - Customer order history
- `GetOrdersByStatusQuery` - Filter by status
- `GetDashboardSummaryQuery` - Dashboard metrics

**Endpoints:**
```
POST   /api/orders/checkout      - Create new order
GET    /api/orders               - List all orders
GET    /api/orders/{id}          - Get order by ID
GET    /api/orders/status/{status} - Filter by status
GET    /api/orders/customer      - Get customer orders
POST   /api/orders/{id}/cancel   - Cancel order
GET    /api/orders/dashboard     - Dashboard summary
GET    /api/products             - List products
GET    /api/products/{id}        - Get product
GET    /api/products/categories  - List categories
```

### 2. InventoryService (Port 5001)
Manages product inventory and stock reservations.

**Models:**
- `InventoryItem` - Stock levels
- `InventoryReservation` - Stock reservations

**Endpoints:**
```
GET    /api/inventory            - List inventory
GET    /api/inventory/{id}       - Get by product ID
GET    /api/inventory/reservations - List reservations
GET    /api/inventory/health     - Health check
```

### 3. PaymentService (Port 5002)
Handles payment processing with simulated transactions.

**Models:**
- `PaymentTransaction` - Transaction records
- `TestCard` - Test card numbers for simulation

**Endpoints:**
```
GET    /api/payment/transactions - List transactions
GET    /api/payment/transactions/{id} - Get transaction
GET    /api/payment/test-cards   - List test cards
GET    /api/payment/health       - Health check
```

### 4. ShippingService (Port 5003)
Manages shipment creation and tracking.

**Models:**
- `Shipment` - Shipment records
- `ShippingCarrier` - Carrier information

**Endpoints:**
```
GET    /api/shipping/shipments  - List shipments
GET    /api/shipping/shipments/{id} - Get shipment
GET    /api/shipping/carriers    - List carriers
GET    /api/shipping/track/{trackingNumber} - Track shipment
GET    /api/shipping/health      - Health check
```

## Event Flow

```
1. OrderSubmittedEvent
   └─► InventoryService checks stock
       ├─► InventoryConfirmedEvent (in stock)
       │   └─► PaymentService processes payment
       │       ├─► PaymentApprovedEvent
       │       │   └─► ShippingService creates shipment
       │       │       └─► ShippingCreatedEvent
       │       │           └─► OrderAPI completes order
       │       └─► PaymentRejectedEvent
       │           └─► OrderAPI marks order failed
       └─► InventoryFailedEvent (out of stock)
           └─► OrderAPI marks order failed
```

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- Docker Desktop
- RabbitMQ

### Running Locally

1. **Start RabbitMQ:**
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

2. **Run Microservices:**
```bash


cd "\SportsStore.OrderAPI"
dotnet run --urls "http://localhost:5138"

cd "\SportsStore.InventoryService"
dotnet run --urls "http://localhost:5139"

cd "d:\shares\full stack 2\fahad\SportsStore.PaymentService"
dotnet run --urls "http://localhost:5140"

cd "d:\shares\full stack 2\fahad\SportsStore.ShippingService"
dotnet run --urls "http://localhost:5141"


```

3. **Run Blazor Portal:**
```bash
cd "d:\shares\full stack 2\fahad\SportsStore.Blazor"
dotnet run --urls "http://localhost:5187"
```

4. **Run React Admin:**
```bash
cd SportsStore.Admin
npm install
npm run dev
```

### Running with Docker Compose

```bash
docker-compose up --build
```



## Testing

### Run All Tests
```bash
dotnet test SportsSln.sln
```

### Test Projects
- `SportsStore.Tests` - Original application tests (Cart, Controllers, TagHelpers)
- `SportsStore.IntegrationTests` - API integration tests
- `SportsStore.OrderAPI.Tests` - OrderAPI unit tests
- `SportsStore.InventoryService.Tests` - Inventory service tests
- `SportsStore.PaymentService.Tests` - Payment service tests
- `SportsStore.ShippingService.Tests` - Shipping service tests

### Test Coverage
Each service has comprehensive tests covering:
- Entity creation and validation
- Database operations
- Business logic scenarios
- API endpoints

## CI/CD Pipeline

GitHub Actions workflow includes:
1. **Build & Test** - Compile and run all tests with coverage
2. **Publish** - Create deployment artifacts
3. **Docker Build** - Build and push container images
4. **React Build** - Build admin dashboard
5. **Integration Tests** - Run integration tests with RabbitMQ

### Required Secrets
- `DOCKER_USERNAME` - Docker Hub username
- `DOCKER_PASSWORD` - Docker Hub password

## Configuration

### appsettings.json
Each service has configuration for:
- Database connection strings
- RabbitMQ settings
- Serilog logging

### Environment Variables
```bash
# RabbitMQ
RabbitMQ__Host=localhost
RabbitMQ__Username=guest
RabbitMQ__Password=guest

# Database
ConnectionStrings__OrderDatabase=Data Source=orderapi.db
```

## Order Status Flow

| Status | Description |
|--------|-------------|
| Cart | Order in shopping cart |
| Submitted | Order placed, pending inventory |
| InventoryPending | Awaiting inventory check |
| InventoryConfirmed | Stock available |
| InventoryFailed | Insufficient stock |
| PaymentPending | Awaiting payment |
| PaymentApproved | Payment successful |
| PaymentFailed | Payment declined |
| ShippingPending | Awaiting shipment |
| ShippingCreated | Shipment dispatched |
| Completed | Order delivered |
| Failed | Order cancelled/failed |

## API Documentation

Access Swagger UI for each service:
- OrderAPI: http://localhost:5000/swagger
- InventoryService: http://localhost:5001/swagger
- PaymentService: http://localhost:5002/swagger
- ShippingService: http://localhost:5003/swagger

## Features

### Blazor Customer Portal
- Product browsing with category filtering
- Shopping cart management
- Order checkout with validation
- Order tracking and history
- Responsive Bootstrap UI

### React Admin Dashboard
- Real-time order management
- Inventory tracking and reservations
- Payment transaction monitoring
- Shipment tracking with carriers
- Status-based filtering
- Responsive sidebar navigation

## License

This project is part of the SportsStore application suite.
