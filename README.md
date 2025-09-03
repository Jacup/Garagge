# Garagge [![.NET](https://github.com/Jacup/Garagge/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Jacup/Garagge/actions/workflows/dotnet.yml) [![Frontend CI](https://github.com/Jacup/Garagge/actions/workflows/vue.yml/badge.svg?branch=main)](https://github.com/Jacup/Garagge/actions/workflows/vue.yml)

> Garagge is a modern vehicle management application built with .NET and Vue.js.


## Table of Contents

- [Garagge  ](#garagge--)
  - [Table of Contents](#table-of-contents)
  - [General Information](#general-information)
  - [Get Started](#get-started)
    - [Prerequisites](#prerequisites)
    - [Quick Start](#quick-start)
    - [Configuration](#configuration)
    - [Usage](#usage)
  - [Development](#development)
    - [Development Setup Modes](#development-setup-modes)
    - [Prerequisites for Development](#prerequisites-for-development)
    - [Setup](#setup)
    - [Frontend Development](#frontend-development)
    - [Backend Development](#backend-development)
    - [Full Stack Development](#full-stack-development)
    - [Development Services](#development-services)
    - [Useful Commands](#useful-commands)
  - [Screenshots](#screenshots)
  - [Contact](#contact)


## General Information

**Garagge** is a comprehensive vehicle management application that allows users to manage their vehicle fleet efficiently. The application is built with a modern tech stack featuring .NET backend API and Vue.js frontend.


## Get Started

### Prerequisites

- Docker and Docker Compose

### Quick Start

1. Download the latest release files from [GitHub Releases](https://github.com/Jacup/Garagge/releases/latest):
   - `docker-compose.yml`
   - `.env`

2. (Recommended) Update security settings in `.env` file:
```bash
# Generate a new JWT secret (recommended for security)
JWT__SECRET=your-new-random-secret-here

# Change default database password
DB_PASSWORD=your-secure-password
```

For detailed configuration options, see [Environment Variables Documentation](docs/ENVIRONMENT_VARIABLES.md).

3. Start the application:
```bash
docker-compose up -d
```

4. Access the application:
   - **Frontend**: http://localhost:4173
   - **Backend API**: http://localhost:5000
   - **Seq Logging**: http://localhost:8082

### Configuration

The `.env` file contains all necessary configuration with working defaults. You can customize:

- **Database credentials** (`DB_USERNAME`, `DB_PASSWORD`)
- **JWT settings** (`JWT__SECRET` - strongly recommended to change)
- **Logging level** (`LOG_LEVEL`)

All other settings have sensible defaults and work out of the box.

### Usage

// TODO: Add usage instructions


## Development

The application provides flexible development environment setup with Docker Compose support. You can run different parts of the stack based on your development needs.

### Development Setup Modes

Garagge supports multiple development modes to match your workflow:

📖 **[Complete Development Setup Guide](docs/DEVELOPMENT_SETUP.md)**

**Quick reference:**
- **Local Development**: Backend + Frontend in IDE, Database in container (`docker-compose up db seq`)
- **Hybrid Frontend**: Frontend + Database in containers, Backend in IDE (`docker-compose up web db seq`)
- **Hybrid Backend**: Backend + Database in containers, Frontend in IDE (`docker-compose up server db seq`)
- **Full Containerized**: Full stack in containers (`docker-compose up`)

See the [Development Setup Guide](docs/DEVELOPMENT_SETUP.md) for detailed instructions, configuration, and troubleshooting.

### Prerequisites for Development

- .NET 8.0 SDK
- Node.js 18+ and npm
- Docker and Docker Compose
- **Recommended IDEs**:
  - **Backend**: JetBrains Rider or Visual Studio
  - **Frontend**: VS Code

### Setup

1. Clone the repository:
```bash
git clone https://github.com/Jacup/Garagge.git
cd Garagge
```

2. The `.env` file is included in the repository with working defaults for development.

### Frontend Development

For frontend-focused development, use **Hybrid Backend** mode where backend and database run in containers while you develop the frontend locally.

📖 **See [Hybrid Backend Setup](docs/DEVELOPMENT_SETUP.md#hybrid-backend-backend--database-in-containers-frontend-in-ide)**

### Backend Development  

For backend-focused development, use **Hybrid Frontend** mode where frontend and database run in containers while you develop the backend locally.

📖 **See [Hybrid Frontend Setup](docs/DEVELOPMENT_SETUP.md#hybrid-frontend-frontend--database-in-containers-backend-in-ide)**

### Full Stack Development

For simultaneous frontend and backend development, use **Local Development** mode where only the database runs in a container.

📖 **See [Local Development Setup](docs/DEVELOPMENT_SETUP.md#local-development-backend--frontend-in-ide-database-in-container)**

### Development Services

Quick reference for development URLs:
- **Frontend**: http://localhost:4173 (container) / http://localhost:5173 (local)
- **Backend API**: http://localhost:5000  
- **Database**: localhost:5432
- **Seq Logging**: http://localhost:8082

📖 **For detailed port and configuration information, see [Development Setup Guide](docs/DEVELOPMENT_SETUP.md#development-tools)**

### Useful Commands

```bash
# Generate API client for frontend (see web/src/api/README.md for details)
cd web
npm run generate:api

# Run backend tests
cd server
dotnet test

# Run frontend tests
cd web
npm run test

# Format frontend code
cd web
npm run format
```

For detailed API client documentation, see [web/src/api/README.md](web/src/api/README.md).


## Screenshots

// TODO: Add screenshots


## Contact

Created by [Jakub Gramburg (@Jacup)](https://github.com/Jacup) - feel free to contact me!
