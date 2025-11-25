# Docker-Only Setup Guide - MMGC System

This project is designed to run **entirely in Docker** - no local .NET SDK installation required!

## Prerequisites

- **Docker** (version 20.10 or later)
- **Docker Compose** (version 2.0 or later)
- That's it! No .NET SDK needed locally.

## Quick Start

### 1. Start the Application

```bash
# Build and start all containers (database + application)
docker-compose up --build

# Or run in detached mode (background)
docker-compose up -d --build
```

### 2. Access the Application

- **Application**: http://localhost:8080
- **SQL Server**: localhost:1433
- **SQLPad (Admin UI)**: http://localhost:3001

### 3. Database Setup

The database is **automatically created** when the application starts! 

The application uses `EnsureCreated()` which:
- ✅ Automatically creates the database if it doesn't exist
- ✅ Creates all tables and schema based on your models
- ✅ Works perfectly in Docker environments
- ✅ No manual migrations needed for initial setup

## How It Works

### Database Auto-Creation

When the `blazor-app` container starts:

1. **Waits for SQL Server** to be healthy (up to 30 seconds)
2. **Connects to the database** with retry logic
3. **Automatically creates** the database schema using `EnsureCreated()`
4. **Starts the application**

You'll see logs like:
```
Checking database connection...
Database connection successful. Ensuring database schema...
Database schema ensured successfully.
```

### Container Architecture

```
┌─────────────────┐
│   blazor-app    │  ← Your Blazor application
│   (Port 8080)   │     - Auto-creates database schema
└────────┬────────┘     - Runs migrations on startup
         │
         │ Network: blazor-network
         │
┌────────▼────────┐
│   mssql-server  │  ← SQL Server 2022
│   (Port 1433)   │     - Persistent data volume
└─────────────────┘     - Health checks enabled
```

## Common Commands

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f blazor-app
docker-compose logs -f mssql
```

### Stop Services

```bash
docker-compose down
```

### Stop and Remove Volumes (⚠️ Deletes Data)

```bash
docker-compose down -v
```

### Restart Services

```bash
docker-compose restart
```

### Rebuild After Code Changes

```bash
docker-compose up --build
```

## Database Management

### Using SQLPad (Web UI)

1. Open http://localhost:3001
2. Login with:
   - **Username**: `sa`
   - **Password**: `YourStrong@Password123`
3. Connect to database:
   - **Server**: `mssql`
   - **Database**: `MedicalManagementSystem`
   - **Username**: `sa`
   - **Password**: `YourStrong@Password123`

### Using Docker Exec

```bash
# Connect to SQL Server container
docker exec -it mssql-server /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Password123 \
  -Q "SELECT name FROM sys.databases"
```

### Backup Database

```bash
# Create backup
docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P YourStrong@Password123 \
  -Q "BACKUP DATABASE MedicalManagementSystem TO DISK = '/var/opt/mssql/backup/MMGC.bak'"
```

## Advanced: Running Migrations (Optional)

If you need to use EF Core migrations instead of `EnsureCreated()`:

```bash
# Run the migration script
./docker-migrate.sh
```

This will:
1. Build a temporary migration container
2. Run `dotnet ef database update`
3. Clean up temporary files

**Note**: The application works fine with `EnsureCreated()` for most scenarios. Migrations are only needed if you want version-controlled schema changes.

## Environment Variables

You can customize the setup using environment variables or a `.env` file:

```bash
# .env file example
MSSQL_SA_PASSWORD=YourStrong@Password123
MSSQL_PORT=1433
BLAZOR_APP_PORT=8080
DB_NAME=MedicalManagementSystem
DB_USER=sa
```

## Troubleshooting

### Database Connection Issues

If you see connection errors:

1. **Check SQL Server is healthy**:
   ```bash
   docker-compose ps
   ```

2. **Check SQL Server logs**:
   ```bash
   docker-compose logs mssql
   ```

3. **Verify network**:
   ```bash
   docker network inspect blazor-network
   ```

### Application Won't Start

1. **Check application logs**:
   ```bash
   docker-compose logs blazor-app
   ```

2. **Rebuild containers**:
   ```bash
   docker-compose down
   docker-compose up --build
   ```

### Database Not Created

The application retries database connection up to 10 times. If it still fails:

1. Ensure SQL Server container is healthy
2. Check connection string in `docker-compose.yml`
3. Verify network connectivity between containers

## Production Considerations

For production deployments:

1. **Change default passwords** in `docker-compose.yml`
2. **Use environment variables** for sensitive data
3. **Set up proper backups** for the database volume
4. **Use `docker-compose.prod.yml`** for production configuration
5. **Consider using EF Core Migrations** instead of `EnsureCreated()`

## File Structure

```
.net/
├── docker-compose.yml          # Main Docker Compose config
├── Dockerfile                  # Application Dockerfile
├── docker-migrate.sh          # Optional migration script
├── MedicalManagementSystem/   # Application source code
│   ├── Models/                # Database models
│   ├── Data/                  # DbContext
│   ├── Services/              # Business logic
│   └── Components/            # Blazor pages
└── DOCKER_SETUP.md            # This file
```

## Support

For issues or questions:
1. Check application logs: `docker-compose logs -f`
2. Verify container status: `docker-compose ps`
3. Review this documentation

---

**Remember**: Everything runs in Docker - no local .NET SDK needed! 🐳

