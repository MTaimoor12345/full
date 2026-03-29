#!/bin/bash
echo "Starting SportsStore All-in-One Container..."

# Create data directories
mkdir -p /data

# NOTE: RabbitMQ is disabled in single container mode
# Services are running in in-memory mode for development

# Start all services in background
echo "Starting OrderAPI on port 5138..."
ASPNETCORE_URLS=http://+:5138 \
ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__OrderDatabase="Data Source=/data/orderapi.db" \
Stripe__SecretKey="sk_test_BQokikJOvBiI2HlWgH4olfQ2" \
RabbitMQ__Host="localhost" \
RabbitMQ__Username="guest" \
RabbitMQ__Password="guest" \
dotnet /app/orderapi/SportsStore.OrderAPI.dll &

echo "Starting InventoryService on port 5139..."
ASPNETCORE_URLS=http://+:5139 \
ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__InventoryDatabase="Data Source=/data/inventory.db" \
RabbitMQ__Host="localhost" \
RabbitMQ__Username="guest" \
RabbitMQ__Password="guest" \
dotnet /app/inventory/SportsStore.InventoryService.dll &

echo "Starting PaymentService on port 5140..."
ASPNETCORE_URLS=http://+:5140 \
ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__PaymentDatabase="Data Source=/data/payment.db" \
RabbitMQ__Host="localhost" \
RabbitMQ__Username="guest" \
RabbitMQ__Password="guest" \
dotnet /app/payment/SportsStore.PaymentService.dll &

echo "Starting ShippingService on port 5141..."
ASPNETCORE_URLS=http://+:5141 \
ASPNETCORE_ENVIRONMENT=Development \
ConnectionStrings__ShippingDatabase="Data Source=/data/shipping.db" \
RabbitMQ__Host="localhost" \
RabbitMQ__Username="guest" \
RabbitMQ__Password="guest" \
dotnet /app/shipping/SportsStore.ShippingService.dll &

# Copy nginx config and start nginx
echo "Starting React Admin on port 3000..."
cp /app/nginx.conf /etc/nginx/sites-available/default
nginx -g "daemon on;"

echo ""
echo "========================================"
echo "SportsStore is running!"
echo "========================================"
echo "React Admin:    http://localhost:3000"
echo "Blazor:         http://localhost:5187"
echo "OrderAPI:       http://localhost:5138"
echo "Inventory:      http://localhost:5139"
echo "Payment:        http://localhost:5140"
echo "Shipping:       http://localhost:5141"
echo "========================================"

# Keep container running
wait
