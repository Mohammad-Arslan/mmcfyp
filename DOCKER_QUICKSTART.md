# Docker Quick Start Guide

## One Command to Start Everything

```bash
docker-compose up --build
```

That's it! The application will:
1. ✅ Build the Blazor application in Docker
2. ✅ Start SQL Server database
3. ✅ Automatically create database schema
4. ✅ Start the application

## Access Points

- **Application**: http://localhost:8080
- **Database Admin (SQLPad)**: http://localhost:3001
- **SQL Server**: localhost:1433

## No Local .NET SDK Needed!

Everything runs in Docker containers:
- ✅ Application builds in Docker
- ✅ Database runs in Docker
- ✅ All dependencies in Docker
- ✅ Zero local installation required

## Database Auto-Creation

The database schema is **automatically created** when the app starts using `EnsureCreated()`. No manual setup needed!

## Common Commands

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild after code changes
docker-compose up --build
```

## Troubleshooting

If the database isn't ready:
- Wait 30-60 seconds for SQL Server to initialize
- Check logs: `docker-compose logs mssql`
- The app will retry automatically (up to 10 times)

See [DOCKER_SETUP.md](DOCKER_SETUP.md) for detailed documentation.
