# Database UI Tools for SQL Server

## Connection Details

Use these credentials to connect to your SQL Server database:

```
Server:     localhost,1433
Username:   mmc (or sa for admin access)
Password:   mmc@12345
Database:   MedicalManagementSystem
```

**Important:** Enable "Trust Server Certificate" in your connection settings.

---

## Option 1: Azure Data Studio (Recommended) ⭐

**Best for:** Cross-platform, official Microsoft tool, free

### Installation
- **Linux:** Download from https://aka.ms/azuredatastudio
- **Windows/Mac:** Also available

### Connection Steps:
1. Open Azure Data Studio
2. Click "New Connection"
3. Enter:
   - **Server:** `localhost,1433`
   - **Authentication Type:** SQL Login
   - **User name:** `mmc`
   - **Password:** `mmc@12345`
   - **Database:** `MedicalManagementSystem`
   - **Trust server certificate:** ✓ (checked)
4. Click "Connect"

---

## Option 2: DBeaver (Free & Cross-platform)

**Best for:** Universal database tool supporting many databases

### Installation
- Download from: https://dbeaver.io/download/
- Available for Linux, Windows, Mac

### Connection Steps:
1. Open DBeaver
2. Click "New Database Connection"
3. Select "Microsoft SQL Server"
4. Enter:
   - **Host:** `localhost`
   - **Port:** `1433`
   - **Database:** `MedicalManagementSystem`
   - **Username:** `mmc`
   - **Password:** `mmc@12345`
5. In "Driver properties", add:
   - `trustServerCertificate` = `true`
6. Click "Test Connection" then "Finish"

---

## Option 3: VS Code Extension

**Best for:** If you already use VS Code

### Installation:
1. Install extension: "SQL Server (mssql)" by Microsoft
2. Press `Ctrl+Shift+P` → "MS SQL: Connect"
3. Enter connection details:
   - Server: `localhost,1433`
   - Database: `MedicalManagementSystem`
   - Username: `mmc`
   - Password: `mmc@12345`
   - Trust Server Certificate: Yes

---

## Option 4: Web-based SQLPad (Included in Docker)

**Best for:** Quick access via browser, no installation needed

### Access:
1. Start the SQLPad container:
   ```bash
   docker compose up -d sqlpad
   ```
2. Open browser: http://localhost:3001
3. Login with:
   - **Email/Username:** `mmc` (or `sa`)
   - **Password:** `mmc@12345`
4. Add connection:
   - **Name:** MedicalManagementSystem
   - **Driver:** Microsoft SQL Server
   - **Host:** `mssql` (use container name, not localhost)
   - **Port:** `1433`
   - **Database:** `MedicalManagementSystem`
   - **Username:** `mmc`
   - **Password:** `mmc@12345`
   - **Trust Server Certificate:** Yes

---

## Option 5: Command Line (sqlcmd)

Already available in your setup:

```bash
# Connect using sa (admin)
docker exec -it mssql-server /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'mmc@12345' -C

# Connect using mmc user
docker exec -it mssql-server /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U mmc -P 'mmc@12345' -C -d MedicalManagementSystem
```

---

## Quick Reference

| Tool | Platform | Cost | Best For |
|------|----------|------|----------|
| Azure Data Studio | All | Free | General use, Microsoft ecosystem |
| DBeaver | All | Free | Multiple database types |
| VS Code Extension | All | Free | VS Code users |
| SQLPad (Web) | Browser | Free | Quick access, no install |
| SSMS | Windows | Free | Windows-only, advanced features |

---

## Troubleshooting

### Connection Refused
- Check if MSSQL container is running: `docker ps | grep mssql-server`
- Verify port 1433 is accessible: `netstat -tuln | grep 1433`

### Authentication Failed
- Verify password in `.env` file
- Try using `sa` user instead of `mmc`
- Check if user exists: Connect as `sa` and run `SELECT name FROM sys.server_principals WHERE name = 'mmc'`

### Trust Server Certificate Error
- Always enable "Trust Server Certificate" option
- Or add `TrustServerCertificate=True` to connection string

---

## Useful SQL Commands

Once connected, try these:

```sql
-- List all databases
SELECT name FROM sys.databases;

-- List all tables in current database
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Check current user
SELECT SUSER_NAME(), USER_NAME();

-- List all users
SELECT name FROM sys.database_principals WHERE type_desc = 'SQL_USER';
```

