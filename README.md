# MMGC - Medical Management & General Care System

A comprehensive medical management system built with Blazor Server and .NET 8.0, designed to run entirely in Docker.

## 🚀 Quick Start (Docker Only)

**No local .NET SDK required!** Everything runs in Docker.

```bash
# Start the application
docker-compose up --build

# Access the application
# http://localhost:8080
```

## 📚 Documentation

- **[Docker Setup Guide](DOCKER_SETUP.md)** - Complete Docker-only setup instructions
- **[Phase I Summary](PHASE1_SUMMARY.md)** - Features and architecture overview
- **[Docker README](README.Docker.md)** - Detailed Docker configuration

## ✨ Features

- ✅ Dashboard with overview cards and statistics
- ✅ Appointments Management (CRUD + Notifications)
- ✅ Doctors Management
- ✅ Patients Management
- ✅ Procedures & Treatments
- ✅ Laboratory Management
- ✅ Transactions & Invoices
- ✅ Reports Module

## 🏗️ Architecture

- **Frontend**: Blazor Server, Bootstrap 5
- **Backend**: ASP.NET Core 8.0, C#
- **Database**: SQL Server 2022 (Docker)
- **ORM**: Entity Framework Core
- **Pattern**: Repository Pattern (DRY Principle)

## 🐳 Docker Services

- `blazor-app` - Main application (Port 8080)
- `mssql-server` - SQL Server database (Port 1433)
- `sqlpad` - Database admin UI (Port 3001)

## 📝 Requirements

- Docker & Docker Compose
- That's it! No local .NET SDK needed.

## 🔧 Development

All development and deployment happens through Docker. See [DOCKER_SETUP.md](DOCKER_SETUP.md) for details.

### 🔥 Hot Reload Enabled!

**Code changes are automatically reflected** - no container restart needed!

```bash
# Start with hot reload (default)
docker-compose up --build

# Make changes to your code
# Save files → Changes appear automatically!
```

See [HOT_RELOAD.md](HOT_RELOAD.md) for detailed hot reload documentation.

---

**Note**: This is Phase I implementation. The system is ready for use and can be extended in Phase II.
