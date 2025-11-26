#!/bin/bash
# Script to check if all Identity tables are created

set -e

echo "=========================================="
echo "Checking ASP.NET Core Identity Tables"
echo "=========================================="
echo ""

# Connection details
DB_SERVER="mssql-server"
DB_NAME="MedicalManagementSystem"
DB_USER="${DB_USER:-mmc}"
DB_PASSWORD="${DB_PASSWORD:-mmc@12345}"

echo "📋 Checking Identity tables in database: $DB_NAME"
echo ""

# SQL script to check tables
SQL_SCRIPT=$(cat <<'EOF'
SET NOCOUNT ON;

DECLARE @Tables TABLE (TableName NVARCHAR(256));
DECLARE @MissingTables TABLE (TableName NVARCHAR(256));

-- Expected Identity tables
INSERT INTO @Tables VALUES 
    ('AspNetRoles'),
    ('AspNetUsers'),
    ('AspNetUserRoles'),
    ('AspNetUserClaims'),
    ('AspNetUserLogins'),
    ('AspNetRoleClaims'),
    ('AspNetUserTokens');

-- Check which tables exist
INSERT INTO @MissingTables (TableName)
SELECT t.TableName
FROM @Tables t
WHERE NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = t.TableName
);

-- Report results
PRINT '=== Identity Tables Status ===';
PRINT '';

DECLARE @Exists INT = 0;
DECLARE @Missing INT = 0;

SELECT @Exists = COUNT(*) 
FROM @Tables t
WHERE EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = t.TableName
);

SELECT @Missing = COUNT(*) FROM @MissingTables;

PRINT 'Tables Found: ' + CAST(@Exists AS NVARCHAR(10));
PRINT 'Tables Missing: ' + CAST(@Missing AS NVARCHAR(10));
PRINT '';

IF @Missing > 0
BEGIN
    PRINT 'Missing Tables:';
    SELECT TableName FROM @MissingTables;
    PRINT '';
    PRINT '❌ Some Identity tables are missing!';
END
ELSE
BEGIN
    PRINT '✅ All Identity tables exist!';
END

PRINT '';
PRINT '=== All Identity Tables ===';
SELECT 
    TABLE_NAME as TableName,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME LIKE 'AspNet%'
ORDER BY TABLE_NAME;
EOF
)

# Execute SQL script
docker exec -i "$DB_SERVER" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U "$DB_USER" \
    -P "$DB_PASSWORD" \
    -d "$DB_NAME" \
    -C \
    <<< "$SQL_SCRIPT"

echo ""
echo "✅ Check completed!"
echo ""

