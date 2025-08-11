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
    - [Environment Setup](#environment-setup)
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

2. (Recommended) Update JWT secret in `.env` file:
```bash
# Generate a new JWT secret (recommended for security)
JWT_SECRET=your-new-random-secret-here
```

3. Start the application:
```bash
docker-compose up -d
```

4. Access the application:
   - **Frontend**: http://localhost:4173
   - **Backend API**: http://localhost:5000
   - **Seq Logging**: http://localhost:8081

### Configuration

The `.env` file contains all necessary configuration with working defaults. You can customize:

- **Database credentials** (`DB_USERNAME`, `DB_PASSWORD`)
- **JWT settings** (`JWT_SECRET` - strongly recommended to change)
- **Logging level** (`LOG_LEVEL`)

All other settings have sensible defaults and work out of the box.

### Usage

// TODO: Add usage instructions


## Development

The application provides flexible development environment setup with Docker Compose support. You can run different parts of the stack based on your development needs.

### Environment Setup

1. Copy and configure environment variables:
```bash
# Root .env file contains database and JWT configuration
cp .env.example .env  # Edit database credentials and JWT secret
```

2. The application uses PostgreSQL database and Seq for logging, both can be run via Docker Compose.

### Frontend Development

When working primarily on the frontend, run the backend via Docker Compose and frontend locally for live development:

```bash
# Start backend services (API + Database + Seq logging)
docker-compose up server db seq

# In separate terminal, start frontend locally
cd garagge-web
npm install
npm run dev
```
Frontend will be available at `http://localhost:5173` and will connect to API at `http://localhost:5000`.

### Backend Development

When working primarily on the backend, run the frontend in Docker to test your API changes:

```bash
# Start only dependencies (Database + Seq + Frontend)
docker-compose up db seq garagge-web

# Run backend in your IDE (Visual Studio/Rider/VS Code)
# or via dotnet CLI:
cd src/Api
dotnet run
```

### Full Stack Development

For working on both frontend and backend simultaneously:

```bash
# Start only infrastructure services
docker-compose up db seq

# Run backend locally
cd src/Api
dotnet run

# In separate terminal, run frontend locally
cd garagge-web
npm run dev
```

### Development Services

- **Backend API**: `http://localhost:5000` (HTTP) / `http://localhost:5001` (HTTPS)
- **Frontend**: `http://localhost:5173` (dev) / `http://localhost:4173` (docker)
- **Database**: `localhost:5432` (PostgreSQL)
- **Seq Logging**: `http://localhost:8081`

### Useful Commands

```bash
# Generate API client for frontend (see garagge-web/src/api/README.md for details)
cd garagge-web
npm run generate:api

# Run backend tests
dotnet test

# Run frontend tests
cd garagge-web
npm run test

# Format frontend code
cd garagge-web
npm run format
```

For detailed API client documentation, see [garagge-web/src/api/README.md](garagge-web/src/api/README.md).


## Screenshots

// TODO: Add screenshots


## Contact

Created by [Jakub Gramburg (@Jacup)](https://github.com/Jacup) - feel free to contact me!
