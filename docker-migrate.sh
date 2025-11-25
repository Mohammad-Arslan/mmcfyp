#!/bin/bash
# Docker-based database migration script
# This script runs EF Core migrations inside a Docker container

set -e

echo "=========================================="
echo "MMGC - Database Migration Script (Docker)"
echo "=========================================="
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if containers exist
if ! docker ps -a | grep -q "blazor-app"; then
    echo "❌ Error: blazor-app container not found."
    echo "   Please run 'docker-compose up -d' first to create the containers."
    exit 1
fi

echo "📦 Building migration container..."
echo ""

# Create a temporary Dockerfile for migrations
cat > Dockerfile.migrate << 'EOF'
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY MedicalManagementSystem/*.csproj MedicalManagementSystem/
COPY MedicalManagementSystem/Models MedicalManagementSystem/Models/
COPY MedicalManagementSystem/Data MedicalManagementSystem/Data/
COPY MedicalManagementSystem/Repositories MedicalManagementSystem/Repositories/
COPY MedicalManagementSystem/Services MedicalManagementSystem/Services/
WORKDIR /app/MedicalManagementSystem
RUN dotnet restore
COPY MedicalManagementSystem/ .
RUN dotnet tool install --global dotnet-ef || true
ENV PATH="${PATH}:/root/.dotnet/tools"
CMD ["dotnet", "ef", "database", "update"]
EOF

# Build migration image
docker build -f Dockerfile.migrate -t mmgc-migrate:latest .

echo ""
echo "🔄 Running database migrations..."
echo ""

# Run migrations
docker run --rm \
    --network blazor-network \
    -e ConnectionStrings__DefaultConnection="Server=mssql,1433;Database=MedicalManagementSystem;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;MultipleActiveResultSets=True" \
    mmgc-migrate:latest \
    dotnet ef database update --verbose

# Cleanup
rm -f Dockerfile.migrate

echo ""
echo "✅ Database migrations completed successfully!"
echo ""
echo "Note: The application uses EnsureCreated() by default, which automatically"
echo "creates the database schema on startup. This migration script is optional"
echo "and can be used for more advanced migration scenarios."

