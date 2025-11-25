# Docker Setup for .NET Core Blazor Application

This directory contains Docker configuration files for running your .NET Core Blazor application in containers.

## Files Overview

- **Dockerfile**: Multi-stage build configuration for building and running the Blazor application
- **.dockerignore**: Excludes unnecessary files from Docker build context
- **docker-compose.yml**: Development environment configuration
- **docker-compose.prod.yml**: Production environment configuration
- **setup.sh**: Automated setup script that builds and starts everything

## Prerequisites

- Docker installed on your system
- Docker Compose (usually included with Docker Desktop)

## Quick Start

### Option 1: Automated Setup (Recommended)

The easiest way to get everything up and running:

```bash
# Make the script executable (if not already)
chmod +x setup.sh

# Run the setup script
./setup.sh
```

The script will:
- Check Docker and Docker Compose installation
- Verify project files exist
- Clean up any existing containers
- Build the Docker image
- Start the containers (including MSSQL database)
- Show container status and access information

Your application will be available at `http://localhost:8080` once the setup completes.
The MSSQL database will be available at `localhost:1433`.

### Option 2: Manual Setup

#### 1. Update the Dockerfile

Before building, ensure the Dockerfile uses your actual project DLL name:

1. Find your `.csproj` file name (e.g., `MedicalManagementSystem.csproj`)
2. Open `Dockerfile` and verify the DLL name on the last line matches your project name

#### 2. Build and Run with Docker Compose (Development)

```bash
# Build and start the container
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop the container
docker-compose down
```

The application will be available at `http://localhost:8080`
The MSSQL database will be available at `localhost:1433`

### 3. Build and Run with Docker Compose (Production)

```bash
# Build and start the production container
docker-compose -f docker-compose.prod.yml up --build -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop the container
docker-compose -f docker-compose.prod.yml down
```

### 4. Build and Run with Docker directly

```bash
# Build the image
docker build -t blazor-app:latest .

# Run the container
docker run -d -p 8080:8080 --name blazor-app blazor-app:latest

# View logs
docker logs -f blazor-app

# Stop and remove the container
docker stop blazor-app
docker rm blazor-app
```

## Project Structure

Your project should have a structure similar to:

```
.net/
├── YourBlazorApp.csproj
├── Program.cs
├── App.razor
├── Pages/
├── Shared/
├── wwwroot/
├── Dockerfile
├── .dockerignore
└── docker-compose.yml
```

## Creating a New Blazor Project

If you don't have a Blazor project yet, you can create one:

```bash
# Create a new Blazor Web App (Server-side rendering)
dotnet new blazor -n YourBlazorApp --interactivity Server

# Or create a Blazor WebAssembly Standalone App
dotnet new blazorwasm -n YourBlazorApp
```

**Note**: The `setup.sh` script will automatically create a Blazor project if one doesn't exist, so manual creation is not required.

After creating the project, remember to update the Dockerfile with your actual DLL name.

## MSSQL Database Configuration

The Docker Compose setup includes a Microsoft SQL Server 2022 container with the following default configuration:

- **Server**: `mssql` (container name) or `localhost` (from host)
- **Port**: `1433`
- **SA Password**: `YourStrong@Password123` (⚠️ **Change this in production!**)
- **Database**: `MedicalManagementSystem` (you may need to create this)
- **EULA**: Accepted automatically

### Connection String

The connection string is automatically configured in the Blazor app container:
```
Server=mssql,1433;Database=MedicalManagementSystem;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;MultipleActiveResultSets=True
```

### Changing the SQL Server Password

**For Development (docker-compose.yml):**
Edit the `MSSQL_SA_PASSWORD` environment variable in `docker-compose.yml`:
```yaml
environment:
  - MSSQL_SA_PASSWORD=YourNewStrong@Password
```

**For Production (docker-compose.prod.yml):**
Set it via environment variable:
```bash
export MSSQL_SA_PASSWORD=YourNewStrong@Password
docker-compose -f docker-compose.prod.yml up -d
```

### Creating the Database

After the containers are running, you can create the database:

```bash
# Connect to SQL Server container
docker exec -it mssql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123

# Create database
CREATE DATABASE MedicalManagementSystem;
GO
```

Or use a SQL client like Azure Data Studio, SQL Server Management Studio, or DBeaver:
- **Server**: `localhost,1433`
- **Authentication**: SQL Server Authentication
- **Username**: `sa`
- **Password**: `YourStrong@Password123`

### Database Persistence

Database data is persisted in a Docker volume (`mssql-data` for development, `mssql-data-prod` for production). To remove all data:

```bash
docker-compose down -v
```

## Environment Variables

You can customize the application behavior using environment variables:

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
- `ASPNETCORE_URLS`: The URL the app listens on (default: `http://+:8080`)
- `ConnectionStrings__DefaultConnection`: SQL Server connection string (automatically set in docker-compose)

Example:
```bash
docker run -d -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ASPNETCORE_URLS=http://+:8080 \
  --name blazor-app blazor-app:latest
```

## Troubleshooting

### Port Already in Use
If port 8080 is already in use, change it in `docker-compose.yml`:
```yaml
ports:
  - "8081:8080"  # Maps host port 8081 to container port 8080
```

If port 1433 (MSSQL) is already in use:
```yaml
ports:
  - "1434:1433"  # Maps host port 1434 to container port 1433
```

### Build Fails
- Ensure your `.csproj` file is in the root directory
- Check that all dependencies are properly referenced
- Verify the .NET SDK version matches (currently using .NET 8.0)

### Application Won't Start
- Check container logs: `docker logs blazor-app`
- Verify the DLL name in Dockerfile matches your project name
- Ensure the port mapping is correct
- Check if MSSQL container is healthy: `docker ps` (should show "healthy" status)

### Database Connection Issues
- Verify MSSQL container is running: `docker ps | grep mssql`
- Check MSSQL logs: `docker logs mssql-server`
- Ensure the database exists (create it if needed)
- Verify connection string matches the password in docker-compose.yml
- Test connection from host: `docker exec -it mssql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Password123 -Q "SELECT @@VERSION"`

## Security Notes

- The Dockerfile runs the application as a non-root user for security
- Production builds exclude development dependencies
- Use `docker-compose.prod.yml` for production deployments
- **⚠️ IMPORTANT**: Change the default MSSQL SA password (`YourStrong@Password123`) before deploying to production
- Consider using Docker secrets or environment files for sensitive credentials in production
- The MSSQL container uses `TrustServerCertificate=True` for development - remove this in production and use proper certificates

