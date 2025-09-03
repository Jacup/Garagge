# Environment Variables

This document describes all environment variables used by Garagge. Variables are defined in the `.env` file in the root directory.

## Quick Start

1. Copy `.env.example` to `.env`
2. Modify values as needed (optional - defaults work out of the box)
3. Run `docker-compose up -d`

## Docker Compose Variables

These variables are used by `docker-compose.yml` and affect container configuration:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `DB_DATA_LOCATION` | Host path where PostgreSQL data is stored | `./postgres-data` | No |

> **Note**: PostgreSQL is not exposed to the host by default for security

## Application Configuration

These variables configure the Garagge application:

### Database Connection

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `DB_HOST` | Database hostname | `db` | No* |
| `DB_PORT` | Database port | `5432` | No |
| `DB_NAME` | Database name | `garagge-db` | No |
| `DB_USERNAME` | Database username | `postgres` | No |
| `DB_PASSWORD` | Database password | `postgres` | **Yes** |

> **Note**: When using the included PostgreSQL container, only `DB_PASSWORD` should be changed for security.

### PostgreSQL Container

When using the included PostgreSQL container, these variables configure the database:

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `POSTGRES_DB` | Initial database name | `garagge-db` | No |
| `POSTGRES_USER` | PostgreSQL superuser name | `postgres` | No |

> **Note**: The PostgreSQL container automatically uses `DB_PASSWORD` as `POSTGRES_PASSWORD`

### Security

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `JWT__SECRET` | JWT signing secret | *random generated* | **Yes** |

> **Warning**: Always change `JWT__SECRET` in production. Use a strong, random string.

### Application Settings

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Production` | No |
| `LOG_LEVEL` | Application log level | `Information` | No |

**Log Levels**: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`

## External Database Setup

To use an external PostgreSQL database instead of the included container:

1. Set database connection variables:
   ```env
   DB_HOST=your-postgres-server.com
   DB_PORT=5432
   DB_NAME=your-database-name
   DB_USERNAME=your-username
   DB_PASSWORD=your-password
   ```

2. Remove or comment out the `db` service in `docker-compose.yml`

3. Remove database-related variables:
   - `POSTGRES_DB`
   - `POSTGRES_USER` 
   - `DB_DATA_LOCATION`

## Development vs Production

### Development Environment

Set `ASPNETCORE_ENVIRONMENT=Development` to:
- Use connection string from `appsettings.json` or `appsettings.Development.json`
- Enable detailed error pages and additional logging
- Allow host override via `DB_HOST` (useful for Docker containers)

**Development in Docker containers:**
```env
ASPNETCORE_ENVIRONMENT=Development
DB_HOST=db  # Override localhost from appsettings.json
```

### Production Environment (Default)

Uses environment variables to build the database connection string securely.

## Security Best Practices

1. **Always change default passwords**:
   - `DB_PASSWORD`
   - `JWT__SECRET`

2. **Use strong, random values**:
   ```bash
   # Generate random JWT secret
   openssl rand -base64 32
   
   # Generate random password
   openssl rand -base64 16
   ```

3. **Restrict file permissions**:
   ```bash
   chmod 600 .env
   ```

4. **Never commit `.env` to version control**

## Troubleshooting

### Database Connection Issues

1. Check if PostgreSQL is running:
   ```bash
   docker-compose logs db
   ```

2. Verify connection variables match between application and PostgreSQL container

3. Ensure `DB_HOST=db` when using the included container

### Permission Errors

```bash
# Fix data directory permissions
sudo chown -R 999:999 ./postgres-data
```
