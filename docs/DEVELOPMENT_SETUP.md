# Development Setup Guide

This guide covers different ways to run Garagge for development, allowing you to run various combinations of services locally or in containers.

## Prerequisites

- Docker and Docker Compose
- .NET SDK (for running backend locally)
- Node.js (for running frontend locally)

## Development Modes

### Local Development: Backend + Frontend in IDE, Database in Container

**Best for:** Full local development with external database

```bash
# Start only database and logging
docker-compose up -d db seq

# Run backend in IDE (Visual Studio, VS Code, Rider)
# Database connection: localhost:5432

# Run frontend in IDE  
cd web
npm run dev
# Frontend: http://localhost:4173 → Backend: http://localhost:5000
```

**Configuration:**
- Backend uses `appsettings.Development.json` with `Host=localhost`
- Frontend uses `.env.development` with `VITE_API_URL=http://localhost:5000`

---

### Hybrid Frontend: Frontend + Database in Containers, Backend in IDE

**Best for:** Backend development with containerized frontend

```bash
# Start frontend, database and logging
docker-compose up -d web db seq

# Run backend in IDE
# Database connection: localhost:5432
# Frontend container: http://localhost:4173 → Backend IDE: http://localhost:5000
```

**Configuration:**
- Backend uses `appsettings.Development.json` with `Host=localhost` 
- Frontend container uses `VITE_API_URL=http://localhost:5000`

---

### Hybrid Backend: Backend + Database in Containers, Frontend in IDE

**Best for:** Frontend development with containerized backend

```bash
# Start backend, database and logging
docker-compose up -d server db seq

# Run frontend in IDE
cd web  
npm run dev
# Frontend IDE: http://localhost:4173 → Backend container: http://localhost:5000
```

**Configuration:**
- Backend container uses `DB_HOST=db` environment variable
- Frontend uses `.env.development` with `VITE_API_URL=http://localhost:5000`

---

### Full Containerized: Full Stack in Containers

**Best for:** Testing complete containerized setup, integration testing

```bash
# Start all services
docker-compose up -d

# Access application: http://localhost:4173
```

**Configuration:**
- Backend container uses `DB_HOST=db` environment variable
- Frontend container uses `VITE_API_URL=http://localhost:5000`
- All services communicate through Docker network

---

## Configuration Details

### Database Connection

| Mode | Backend Location | Database Host | Database Port |
|------|------------------|---------------|---------------|
| Local Development | IDE | `localhost` | `5432` |
| Hybrid Frontend | IDE | `localhost` | `5432` |
| Hybrid Backend | Container | `db` | `5432` |
| Full Containerized | Container | `db` | `5432` |

### API Communication

| Mode | Frontend Location | Backend URL |
|------|-------------------|-------------|
| Local Development | IDE | `http://localhost:5000` |
| Hybrid Frontend | Container | `http://localhost:5000` |
| Hybrid Backend | IDE | `http://localhost:5000` |
| Full Containerized | Container | `http://localhost:5000` |

> **Note:** Frontend always connects to backend via `localhost:5000` because JavaScript runs in the browser, not in the container.

## Development Tools

### Database Access

PostgreSQL is exposed on `localhost:5432` in all modes for database tools:

```bash
# Connection details
Host: localhost
Port: 5432
Database: garagge-db
Username: postgres
Password: postgres  # Change in .env for security
```

### Logging

Seq logging is available at `http://localhost:8082` in all modes.

### Hot Reload

- **Backend (IDE):** Automatic with IDE debugging
- **Frontend (IDE):** `npm run dev` provides hot reload
- **Containers:** Rebuild required for changes

## Troubleshooting

### Database Connection Issues

```bash
# Check if database is running
docker-compose ps db

# Check database logs
docker-compose logs db

# Test connection
psql -h localhost -p 5432 -U postgres -d garagge-db
```

### Port Conflicts

If you encounter port conflicts, you can change ports in `.env`:

```env
# Change PostgreSQL host port
POSTGRES_HOST_PORT=5433

# Remember to update your connection strings accordingly
```

### Frontend API Connection Issues

1. Ensure backend is running on port 5000
2. Check CORS settings in backend
3. Verify `VITE_API_URL` in frontend configuration

### Container Build Issues

```bash
# Clean rebuild
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Best Practices

1. **Use Local Development** for active development of both frontend and backend
2. **Use Hybrid Frontend** when focusing on backend development
3. **Use Hybrid Backend** when focusing on frontend development  
4. **Use Full Containerized** for integration testing and deployment validation

## Environment Variables

See [Environment Variables Documentation](ENVIRONMENT_VARIABLES.md) for complete configuration options.
