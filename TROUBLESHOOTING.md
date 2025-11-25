# Troubleshooting Guide

## SQL Server Connection Errors

### Error 17836: Network Packet Payload Mismatch

**Symptoms:**
```
Error: 17836, Severity: 20, State: 17.
Length specified in network packet payload did not match number of bytes read
```

**Solution Applied:**
The connection string has been updated with optimized parameters:
- `Packet Size=4096` - Sets consistent packet size
- `Connection Timeout=30` - Allows time for connections
- `Command Timeout=30` - Prevents command timeouts
- `Pooling=true` - Enables connection pooling
- `Encrypt=False` - Disables encryption for Docker internal network
- `TrustServerCertificate=True` - Trusts server certificate

**Additional Fixes:**
- Added EF Core retry logic (5 retries with 30-second delay)
- Improved error handling in database initialization
- Connection pooling configured properly

**If errors persist:**

1. **Check SQL Server is healthy:**
   ```bash
   docker-compose ps
   docker-compose logs mssql
   ```

2. **Restart containers:**
   ```bash
   docker-compose restart blazor-app
   ```

3. **Check network connectivity:**
   ```bash
   docker exec blazor-app ping mssql
   ```

4. **Verify connection string:**
   ```bash
   docker exec blazor-app env | grep ConnectionStrings
   ```

## Hot Reload Issues

### Changes Not Reflecting

1. **Check file watcher:**
   ```bash
   docker-compose logs blazor-app | grep -i watch
   ```

2. **Verify volume mount:**
   ```bash
   docker exec blazor-app ls -la /app/MedicalManagementSystem
   ```

3. **Restart watch:**
   ```bash
   docker-compose restart blazor-app
   ```

## Database Connection Issues

### Database Not Found

The application automatically creates the database on startup. If it doesn't:

1. **Check database exists:**
   ```bash
   docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd \
     -S localhost -U sa -P YourStrong@Password123 \
     -Q "SELECT name FROM sys.databases" -C
   ```

2. **Create manually if needed:**
   ```bash
   docker exec mssql-server /opt/mssql-tools18/bin/sqlcmd \
     -S localhost -U sa -P YourStrong@Password123 \
     -Q "CREATE DATABASE MedicalManagementSystem" -C
   ```

## Build Issues

### Docker Build Fails

1. **Clean build:**
   ```bash
   docker-compose down
   docker-compose build --no-cache
   docker-compose up
   ```

2. **Check Dockerfile:**
   ```bash
   docker build -f Dockerfile.dev -t test-build .
   ```

## Performance Issues

### Slow Hot Reload

- First build is slower (normal)
- Subsequent builds are incremental (faster)
- Build artifacts are cached in anonymous volumes

### High Memory Usage

- Development mode uses more memory (normal)
- Use production Dockerfile for lower memory usage

## Common Solutions

### Reset Everything

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (⚠️ deletes data)
docker-compose down -v

# Rebuild and start
docker-compose up --build
```

### View All Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f blazor-app
docker-compose logs -f mssql
```

### Check Container Status

```bash
docker-compose ps
docker stats
```

## Getting Help

1. Check logs first: `docker-compose logs`
2. Verify container status: `docker-compose ps`
3. Check network: `docker network inspect blazor-network`
4. Review this troubleshooting guide
5. Check [DOCKER_SETUP.md](DOCKER_SETUP.md) for setup issues

