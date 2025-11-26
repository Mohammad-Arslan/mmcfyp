#!/bin/bash
# Script to create EF Core migration for Identity tables

set -e

echo "=========================================="
echo "Creating EF Core Migration for Identity"
echo "=========================================="
echo ""

# Check if container is running
if ! docker ps | grep -q blazor-app; then
    echo "❌ Error: blazor-app container is not running"
    echo "Please start it with: docker-compose up -d"
    exit 1
fi

echo "📋 Creating migration: InitialIdentityMigration"
echo ""

# Create migration inside the container
docker exec blazor-app dotnet ef migrations add InitialIdentityMigration \
    --project MedicalManagementSystem/MedicalManagementSystem.csproj \
    --context ApplicationDbContext \
    --output-dir Data/Migrations

echo ""
echo "✅ Migration created successfully!"
echo ""
echo "📝 Next steps:"
echo "   1. The migration will be applied automatically on next app startup"
echo "   2. Or apply manually: docker exec blazor-app dotnet ef database update --project MedicalManagementSystem/MedicalManagementSystem.csproj"
echo ""

