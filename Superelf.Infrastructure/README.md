# PostgreSQL Migration Guide

This document provides instructions for migrating from SQL Server to PostgreSQL in the Superelf application.

## Overview

The Superelf application has been updated to use PostgreSQL instead of SQL Server. This README provides information on how to:

1. Set up PostgreSQL locally for development
2. Configure the application to connect to PostgreSQL
3. Run the migrations to create the database schema
4. Migrate existing data (if needed)
5. Troubleshoot common issues

## Local Development Setup

### 1. Install PostgreSQL

Download and install PostgreSQL from the official website: https://www.postgresql.org/download/

**Recommended version:** PostgreSQL 15.x or later

During installation:
- Set username to: `postgres`
- Set password to: `postgres` (for local development only)
- Keep the default port: `5432`

### 2. Create the Database

You can create the database manually using pgAdmin or run the following commands:

```bash
# Connect to PostgreSQL
psql -U postgres

# Create the database
CREATE DATABASE "SuperelfDb";

# Exit
\q
```

### 3. Configure Connection String

The connection strings have already been updated in the appsettings files:

- **Development:** `Host=localhost;Port=5432;Database=SuperelfDb;Username=postgres;Password=postgres`
- **Test:** `Host=postgres-test;Port=5432;Database=SuperelfDbTest;Username=postgres;Password=${TEST_DB_PASSWORD}`
- **Production:** `Host=postgres-prod;Port=5432;Database=SuperelfDb;Username=postgres;Password=${PROD_DB_PASSWORD}`

## Running Migrations

The application will automatically run the migrations when it starts up. However, if you want to run migrations manually:

```bash
# Navigate to the API project directory
cd Superelf.API

# Run migrations
dotnet ef database update --project ../Superelf.Infrastructure
```

## Docker Environment

For Docker environments, the following changes have been made:

- SQL Server containers have been replaced with PostgreSQL containers
- Container names and network configurations have been updated
- Volumes have been updated to store PostgreSQL data
- Migration process has been simplified to be more reliable in Docker

### Running in Docker

```bash
# For test environment
docker-compose -f docker-compose.test.yml up -d

# For production environment
docker-compose -f docker-compose.prod.yml up -d
```

### Docker Configuration Structure

The Docker configuration follows these principles:

1. The PostgreSQL database service has health checks to ensure it's ready before other services use it
2. The migration service depends on the database being healthy and runs only once
3. The API service depends on the migration service completing successfully

This setup ensures that the database is properly set up before the application tries to use it, avoiding common errors with database availability in containerized environments.

## Migrating Existing Data

If you need to migrate existing data from SQL Server to PostgreSQL, you can use one of these approaches:

### Option 1: Using the Provided Migration Script

Run the provided PowerShell script:

```powershell
.\migrate-sql-to-postgres.ps1
```

### Option 2: Manual Export/Import

1. Export data from SQL Server using SQL Server Management Studio (SSMS)
2. Transform the data as needed for PostgreSQL compatibility
3. Import the data into PostgreSQL

### Option 3: Using pgLoader

[pgLoader](https://github.com/dimitri/pgloader) is a specialized tool for migrating data to PostgreSQL:

```bash
pgloader mssql://user:password@host/dbname postgresql://postgres:postgres@localhost/SuperelfDb
```

## Entity Framework Core Configuration

The application is already configured to detect the database type based on the connection string format:

- If the connection string contains `Host=`, it uses PostgreSQL
- Otherwise, it uses SQL Server

This logic is implemented in `Program.cs`.

## PostgreSQL-Specific Considerations

When working with PostgreSQL, keep in mind:

1. **Case Sensitivity**: PostgreSQL treats table and column names as case-sensitive unless they're quoted
2. **Schema**: PostgreSQL uses `public` as the default schema instead of `dbo`
3. **Reserved Keywords**: Some SQL Server keywords may be reserved in PostgreSQL
4. **Functions**: PostgreSQL has different syntax for some functions:
   - Use `now()` instead of `GETDATE()`
   - Use `current_date` instead of `GETUTCDATE()`

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Ensure PostgreSQL is running
   - Check if the port (5432) is open and accessible

2. **Authentication Failed**
   - Verify username and password
   - Check PostgreSQL's `pg_hba.conf` file for authentication settings

3. **Database Does Not Exist**
   - Create the database manually before running the application

4. **Migration Errors**
   - Run `dotnet ef migrations add InitialMigration --project ../Superelf.Infrastructure` to create a new migration if needed
   - If migration fails due to existing tables, you may need to drop the database and recreate it

### Logs

Check the application logs for detailed error messages. The application is configured to log database connection issues and migration errors.

## Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
