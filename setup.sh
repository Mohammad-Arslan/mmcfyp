#!/bin/bash

set -euo pipefail  # Exit on error, undefined vars, pipe failures

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}  .NET Core Blazor Docker Setup${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Function to print error messages
print_error() {
    echo -e "${RED}✗ $1${NC}" >&2
}

# Function to print success messages
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# Function to print info messages
print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

# Function to print warning messages
print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

# Load environment variables from .env file if it exists
load_env() {
    if [ -f ".env" ]; then
        print_info "Loading environment variables from .env file..."
        set -a
        source .env
        set +a
        print_success "Environment variables loaded"
    else
        print_warning ".env file not found. Using default values."
        print_info "Creating .env.example file for reference..."
        create_env_example
    fi
}

# Create .env.example file
create_env_example() {
    if [ ! -f ".env.example" ]; then
        cat > .env.example << 'EOF'
# Database Configuration
MSSQL_SA_PASSWORD=YourStrong@Password123
DB_NAME=MedicalManagementSystem
DB_USER=sa

# Application Configuration
BLAZOR_APP_PORT=8080
MSSQL_PORT=1433

# Project Configuration
PROJECT_NAME=MedicalManagementSystem
PROJECT_DIR=MedicalManagementSystem
EOF
        print_success "Created .env.example file"
    fi
}

# Set default values if not provided
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong@Password123}"
DB_NAME="${DB_NAME:-MedicalManagementSystem}"
DB_USER="${DB_USER:-sa}"
BLAZOR_APP_PORT="${BLAZOR_APP_PORT:-8080}"
MSSQL_PORT="${MSSQL_PORT:-1433}"
PROJECT_NAME="${PROJECT_NAME:-MedicalManagementSystem}"
PROJECT_DIR="${PROJECT_DIR:-MedicalManagementSystem}"

# Load environment variables
load_env

# Check if Docker is installed
print_info "Checking Docker installation..."
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    echo "Visit: https://docs.docker.com/get-docker/"
    exit 1
fi
print_success "Docker is installed"

# Check if Docker is running
if ! docker info &> /dev/null; then
    print_error "Docker is not running. Please start Docker first."
    exit 1
fi
print_success "Docker is running"

# Check if Docker Compose is available
print_info "Checking Docker Compose..."
if docker compose version &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker compose"
    print_success "Docker Compose (plugin) is available"
elif command -v docker-compose &> /dev/null; then
    DOCKER_COMPOSE_CMD="docker-compose"
    print_success "Docker Compose (standalone) is available"
else
    print_error "Docker Compose is not available. Please install Docker Compose."
    exit 1
fi

# Check if project files exist
print_info "Checking project files..."
if [ ! -f "Dockerfile" ]; then
    print_error "Dockerfile not found in current directory."
    exit 1
fi

if [ ! -f "docker-compose.yml" ]; then
    print_error "docker-compose.yml not found in current directory."
    exit 1
fi

# Validate docker-compose.yml structure
print_info "Validating docker-compose.yml..."
if ! grep -q "blazor-app:" docker-compose.yml && ! grep -q "blazor-app" docker-compose.yml; then
    print_error "docker-compose.yml does not contain 'blazor-app' service"
    exit 1
fi

if ! grep -q "mssql:" docker-compose.yml && ! grep -q "mssql-server:" docker-compose.yml; then
    print_error "docker-compose.yml does not contain 'mssql' or 'mssql-server' service"
    exit 1
fi
print_success "docker-compose.yml structure validated"

# Check port availability
check_port() {
    local port=$1
    local service=$2
    if command -v lsof &> /dev/null; then
        if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
            print_warning "Port $port is already in use. $service may not start correctly."
            print_info "You can change the port in .env file (BLAZOR_APP_PORT or MSSQL_PORT)"
            return 1
        fi
    elif command -v netstat &> /dev/null; then
        if netstat -tuln 2>/dev/null | grep -q ":$port "; then
            print_warning "Port $port is already in use. $service may not start correctly."
            print_info "You can change the port in .env file (BLAZOR_APP_PORT or MSSQL_PORT)"
            return 1
        fi
    fi
    return 0
}

print_info "Checking port availability..."
check_port "$BLAZOR_APP_PORT" "Blazor app" || true
check_port "$MSSQL_PORT" "MSSQL server" || true

# Determine project location and .csproj file
CSPROJ_FILE=""
PROJECT_PATH=""

# Check if project exists in subdirectory
if [ -d "$PROJECT_DIR" ] && [ -f "$PROJECT_DIR/${PROJECT_NAME}.csproj" ]; then
    PROJECT_PATH="$PROJECT_DIR"
    CSPROJ_FILE="$PROJECT_DIR/${PROJECT_NAME}.csproj"
    print_success "Found project in directory: $PROJECT_DIR"
elif [ -f "${PROJECT_NAME}.csproj" ]; then
    PROJECT_PATH="."
    CSPROJ_FILE="${PROJECT_NAME}.csproj"
    print_success "Found project file: $CSPROJ_FILE"
elif [ -f "$PROJECT_DIR/${PROJECT_NAME}.csproj" ]; then
    PROJECT_PATH="$PROJECT_DIR"
    CSPROJ_FILE="$PROJECT_DIR/${PROJECT_NAME}.csproj"
    print_success "Found project file: $CSPROJ_FILE"
else
    # Try to find any .csproj file
    CSPROJ_FILE=$(find . -maxdepth 2 -name "*.csproj" | head -n 1)
    if [ -n "$CSPROJ_FILE" ]; then
        PROJECT_PATH=$(dirname "$CSPROJ_FILE")
        if [ "$PROJECT_PATH" = "." ]; then
            PROJECT_PATH="."
        fi
        print_success "Found project file: $CSPROJ_FILE"
    fi
fi

# Create project if it doesn't exist
if [ -z "$CSPROJ_FILE" ] || [ ! -f "$CSPROJ_FILE" ]; then
    print_warning "No .csproj file found."
    print_info "Creating a new Blazor Server project: $PROJECT_NAME"
    
    # Check if .NET SDK is installed locally
    if command -v dotnet &> /dev/null; then
        print_success ".NET SDK is installed locally"
        
        # Create the Blazor Web App project (supports server-side rendering)
        if dotnet new blazor -n "$PROJECT_NAME" --framework net8.0 --interactivity Server; then
            print_success "Blazor project created successfully"
            
            # Keep project in its folder - DO NOT flatten
            if [ -d "$PROJECT_NAME" ]; then
                PROJECT_PATH="$PROJECT_NAME"
                CSPROJ_FILE="$PROJECT_NAME/${PROJECT_NAME}.csproj"
                print_success "Project created in directory: $PROJECT_NAME"
                print_info "Project structure preserved (not flattened)"
            fi
        else
            print_error "Failed to create Blazor project"
            exit 1
        fi
    else
        print_info ".NET SDK not found locally. Using Docker to create project..."
        # Use Docker to create the project
        if docker run --rm -v "$(pwd):/app" -w /app mcr.microsoft.com/dotnet/sdk:8.0 \
            dotnet new blazor -n "$PROJECT_NAME" --framework net8.0 --interactivity Server; then
            print_success "Blazor project created successfully using Docker"
            
            # Keep project in its folder - DO NOT flatten
            if [ -d "$PROJECT_NAME" ]; then
                PROJECT_PATH="$PROJECT_NAME"
                CSPROJ_FILE="$PROJECT_NAME/${PROJECT_NAME}.csproj"
                print_success "Project created in directory: $PROJECT_NAME"
                print_info "Project structure preserved (not flattened)"
            fi
        else
            print_error "Failed to create project using Docker"
            print_info "Please ensure Docker is running and try again"
            exit 1
        fi
    fi
fi

# Verify the project file exists
if [ -z "$CSPROJ_FILE" ] || [ ! -f "$CSPROJ_FILE" ]; then
    print_error "Failed to create or locate project file"
    exit 1
fi

# Check if project is Blazor WebAssembly (not supported)
print_info "Checking project type..."
if grep -q "Sdk=\"Microsoft.NET.Sdk.BlazorWebAssembly\"" "$CSPROJ_FILE" || \
   grep -q "Microsoft.NET.Sdk.BlazorWebAssembly" "$CSPROJ_FILE"; then
    print_error "This script only supports Blazor Server projects."
    print_error "Blazor WebAssembly projects are not supported for server-side Docker deployment."
    print_info "Please use a Blazor Server project instead."
    exit 1
fi

# Verify it's a Blazor Server project
if ! grep -q "Microsoft.NET.Sdk.BlazorWebApp\|Microsoft.NET.Sdk.Razor\|Microsoft.NET.Sdk.Web" "$CSPROJ_FILE"; then
    print_warning "Project may not be a Blazor project. Continuing anyway..."
else
    print_success "Project type validated: Blazor Server"
fi

print_success "Project ready: $CSPROJ_FILE"

# Update docker-compose.yml context if project is in subdirectory
if [ "$PROJECT_PATH" != "." ] && [ -d "$PROJECT_PATH" ]; then
    print_info "Project is in subdirectory. Updating Dockerfile context..."
    # Note: This assumes the Dockerfile can handle the subdirectory structure
    # The Dockerfile should be updated to COPY from the correct path
    print_info "Ensure Dockerfile copies from: $PROJECT_PATH"
fi

# Stop and remove existing containers if they exist
print_info "Cleaning up existing containers..."

# List of containers to clean up (matching docker-compose.yml container names)
CONTAINERS=("blazor-app" "mssql-server" "sqlpad")

for container in "${CONTAINERS[@]}"; do
    if docker ps -a --format '{{.Names}}' | grep -q "^${container}$"; then
        print_info "Stopping existing ${container} container..."
        docker stop "${container}" 2>/dev/null || true
        docker rm "${container}" 2>/dev/null || true
        print_success "Removed existing ${container} container"
    else
        print_info "No existing ${container} container found (skipping)"
    fi
done

# Build and start containers
echo ""
print_info "Building Docker image..."
if $DOCKER_COMPOSE_CMD build --no-cache; then
    print_success "Docker image built successfully"
else
    print_error "Failed to build Docker image"
    print_info "Showing build logs..."
    $DOCKER_COMPOSE_CMD build 2>&1 | tail -50
    exit 1
fi

echo ""
print_info "Starting containers..."
if $DOCKER_COMPOSE_CMD up -d; then
    print_success "Containers started successfully"
else
    print_error "Failed to start containers"
    print_info "Showing startup logs..."
    $DOCKER_COMPOSE_CMD logs --tail=50
    exit 1
fi

# Wait a moment for the containers to start
sleep 3

# Check container status
echo ""
print_info "Checking container status..."

# Check MSSQL container
MSSQL_RUNNING=false
if docker ps --format '{{.Names}}' | grep -q "^mssql-server$"; then
    print_success "MSSQL container is running"
    MSSQL_RUNNING=true
    
    # Wait for MSSQL to be healthy
    print_info "Waiting for MSSQL to be ready (this may take 30-60 seconds)..."
    MAX_WAIT=90
    WAIT_COUNT=0
    while [ $WAIT_COUNT -lt $MAX_WAIT ]; do
        HEALTH_STATUS=$(docker inspect --format='{{.State.Health.Status}}' mssql-server 2>/dev/null || echo "starting")
        if [ "$HEALTH_STATUS" = "healthy" ]; then
            print_success "MSSQL is healthy and ready"
            break
        fi
        sleep 2
        WAIT_COUNT=$((WAIT_COUNT + 2))
        echo -n "."
    done
    echo ""
    
    if [ "$HEALTH_STATUS" != "healthy" ]; then
        print_warning "MSSQL health check timeout. It may still be starting."
        print_info "Check logs with: docker logs mssql-server"
    fi
    
    # Create database user if it doesn't exist (and is not 'sa')
    if [ "$DB_USER" != "sa" ]; then
        print_info "Checking if database user '$DB_USER' exists..."
        sleep 2
        if docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd \
            -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
            -Q "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '$DB_USER') CREATE LOGIN [$DB_USER] WITH PASSWORD = '$MSSQL_SA_PASSWORD', DEFAULT_DATABASE = [master], CHECK_POLICY = OFF" \
            -C 2>/dev/null; then
            print_success "Database user '$DB_USER' is ready"
        else
            print_warning "Could not create database user '$DB_USER'. It may already exist."
        fi
    fi
    
    # Create database if it doesn't exist
    print_info "Checking if database '$DB_NAME' exists..."
    sleep 2
    if docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
        -Q "IF DB_ID('$DB_NAME') IS NULL CREATE DATABASE [$DB_NAME]" \
        -C 2>/dev/null; then
        print_success "Database '$DB_NAME' is ready"
        
        # Grant permissions to DB_USER if it's not 'sa'
        if [ "$DB_USER" != "sa" ]; then
            print_info "Granting permissions to user '$DB_USER' on database '$DB_NAME'..."
            sleep 1
            docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd \
                -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
                -Q "USE [$DB_NAME]; IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '$DB_USER') CREATE USER [$DB_USER] FOR LOGIN [$DB_USER]; ALTER ROLE db_owner ADD MEMBER [$DB_USER];" \
                -C 2>/dev/null && print_success "Permissions granted to '$DB_USER'" || print_warning "Could not grant permissions (user may already have access)"
        fi
    else
        print_warning "Could not create database automatically. You may need to create it manually."
        print_info "To create manually: docker exec -it mssql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$MSSQL_SA_PASSWORD' -C -Q \"CREATE DATABASE [$DB_NAME]\""
    fi
else
    print_warning "MSSQL container may not be running"
fi

# Check Blazor app container
if docker ps --format '{{.Names}}' | grep -q "^blazor-app$"; then
    print_success "Blazor app container is running"
    
    # Show container info
    echo ""
    echo -e "${BLUE}Container Information:${NC}"
    docker ps --filter "name=blazor-app" --filter "name=mssql-server" --filter "name=sqlpad" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    
    # Check if the application is responding
    echo ""
    print_info "Waiting for application to be ready..."
    MAX_APP_WAIT=60
    APP_WAIT_COUNT=0
    APP_READY=false
    
    while [ $APP_WAIT_COUNT -lt $MAX_APP_WAIT ]; do
        HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:$BLAZOR_APP_PORT" 2>/dev/null || echo "000")
        if echo "$HTTP_CODE" | grep -qE "200|301|302|307"; then
            print_success "Application is responding (HTTP $HTTP_CODE)"
            APP_READY=true
            break
        fi
        
        # Try health endpoint if available
        HEALTH_CODE=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:$BLAZOR_APP_PORT/health" 2>/dev/null || echo "000")
        if echo "$HEALTH_CODE" | grep -qE "200|204"; then
            print_success "Application health check passed (HTTP $HEALTH_CODE)"
            APP_READY=true
            break
        fi
        
        sleep 2
        APP_WAIT_COUNT=$((APP_WAIT_COUNT + 2))
        echo -n "."
    done
    echo ""
    
    if [ "$APP_READY" = false ]; then
        print_warning "Application may still be starting or there may be an issue."
        print_info "Check logs with: $DOCKER_COMPOSE_CMD logs -f blazor-app"
        print_info "Check container status: docker ps -a | grep blazor-app"
    fi
    
    echo ""
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}  Setup Complete!${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    echo -e "${BLUE}Application Access:${NC}"
    echo -e "  Blazor App:       ${GREEN}http://localhost:$BLAZOR_APP_PORT${NC}"
    echo -e "  MSSQL Server:     ${GREEN}localhost:$MSSQL_PORT${NC}"
    echo ""
    echo -e "${BLUE}Database Connection:${NC}"
    echo -e "  Server:           ${YELLOW}localhost,$MSSQL_PORT${NC}"
    echo -e "  Username:         ${YELLOW}$DB_USER${NC}"
    echo -e "  Password:         ${YELLOW}$MSSQL_SA_PASSWORD${NC}"
    echo -e "  Database:         ${YELLOW}$DB_NAME${NC}"
    echo ""
    echo -e "${BLUE}Useful commands:${NC}"
    echo -e "  View logs:        ${YELLOW}$DOCKER_COMPOSE_CMD logs -f${NC}"
    echo -e "  View app logs:    ${YELLOW}$DOCKER_COMPOSE_CMD logs -f blazor-app${NC}"
    echo -e "  View MSSQL logs:  ${YELLOW}docker logs mssql-server${NC}"
    echo -e "  Stop containers:  ${YELLOW}$DOCKER_COMPOSE_CMD down${NC}"
    echo -e "  Restart:          ${YELLOW}$DOCKER_COMPOSE_CMD restart${NC}"
    echo -e "  View status:      ${YELLOW}docker ps${NC}"
    echo -e "  Connect to SQL:   ${YELLOW}docker exec -it mssql-server /opt/mssql-tools18/bin/sqlcmd -S localhost -U $DB_USER -P '$MSSQL_SA_PASSWORD' -C${NC}"
    echo ""
    
else
    print_error "Blazor app container failed to start"
    echo ""
    print_info "Checking logs for errors..."
    $DOCKER_COMPOSE_CMD logs --tail=100 blazor-app
    echo ""
    print_info "All container logs:"
    $DOCKER_COMPOSE_CMD logs --tail=50
    exit 1
fi
