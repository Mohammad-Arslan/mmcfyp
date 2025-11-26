#!/bin/bash
# Script to add Identity tables to existing database using SQL scripts
# This preserves all existing data

set -e

echo "=========================================="
echo "Adding ASP.NET Core Identity Tables"
echo "=========================================="
echo ""

# Connection details
DB_SERVER="mssql-server"
DB_NAME="${DB_NAME:-MedicalManagementSystem}"
DB_USER="${DB_USER:-mmc}"
DB_PASSWORD="${DB_PASSWORD:-mmc@12345}"

echo "📋 Creating Identity tables in existing database..."
echo ""

# SQL script to create Identity tables
SQL_SCRIPT=$(cat <<'EOF'
-- Create AspNetRoles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
    PRINT 'Created AspNetRoles table';
END
ELSE
    PRINT 'AspNetRoles table already exists';

-- Create AspNetUsers table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        [FirstName] nvarchar(max) NOT NULL DEFAULT '',
        [LastName] nvarchar(max) NOT NULL DEFAULT '',
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [LastLoginAt] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers]([NormalizedEmail]);
    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers]([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
    PRINT 'Created AspNetUsers table';
END
ELSE
    PRINT 'AspNetUsers table already exists';

-- Create AspNetUserRoles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]([RoleId]);
    PRINT 'Created AspNetUserRoles table';
END
ELSE
    PRINT 'AspNetUserRoles table already exists';

-- Create AspNetUserClaims table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY(1,1),
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]([UserId]);
    PRINT 'Created AspNetUserClaims table';
END
ELSE
    PRINT 'AspNetUserClaims table already exists';

-- Create AspNetUserLogins table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]([UserId]);
    PRINT 'Created AspNetUserLogins table';
END
ELSE
    PRINT 'AspNetUserLogins table already exists';

-- Create AspNetRoleClaims table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY(1,1),
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]([RoleId]);
    PRINT 'Created AspNetRoleClaims table';
END
ELSE
    PRINT 'AspNetRoleClaims table already exists';

-- Create AspNetUserTokens table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'Created AspNetUserTokens table';
END
ELSE
    PRINT 'AspNetUserTokens table already exists';

PRINT '';
PRINT '✅ Identity tables creation completed!';
PRINT 'The application will now create roles and admin user on next startup.';
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
echo "✅ Identity tables have been added to the database!"
echo ""
echo "📝 Next steps:"
echo "   1. Restart the blazor-app container: docker restart blazor-app"
echo "   2. The application will automatically create roles and admin user"
echo "   3. Login with: admin@mmgc.com / Arsal@112231"
echo ""

