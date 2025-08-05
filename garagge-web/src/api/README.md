# API Client - Orval Generated

This project uses [Orval](https://orval.dev/) to automatically generate a TypeScript API client based on the OpenAPI specification from a .NET backend.

## Configuration

### Configuration files:
- `orval.config.ts` – main Orval configuration
- `src/api/axios-instance.ts` – custom axios instance with interceptors

### Environments:
- **Development**: `http://localhost:5000/openapi/v1.json`
- **Production**: use the `OPENAPI_INPUT` environment variable

## Usage

### Generate API client

```bash
# One-time generation
npm run generate:api

# Automatic generation on changes (watch mode)
npm run generate:api:watch
```

### Import and usage

```typescript
// Import generated types and functions
import { getVehicles, type VehicleDto, type CreateMyVehicleCommand } from '@/api/generated'

// Or use ready-made wrappers
import { getMyVehicles, addNewVehicle } from '@/api/vehiclesApiNew'

// Example usage
const vehicles = await getMyVehicles()
const newVehicle = await addNewVehicle({
  brand: 'Toyota',
  model: 'Corolla',
  manufacturedYear: 2023
})
```

## Generated files structure

```
src/api/generated/
├── index.ts              # Exports all types and functions
├── apiV1.schemas.ts      # TypeScript interfaces from OpenAPI
├── vehicles/
│   └── vehicles.ts       # API functions for vehicles
└── users/
    └── users.ts          # API functions for users
```

## Benefits of automatic generation

✅ **Automatic synchronization** – types always match the backend  
✅ **Type safety** – full TypeScript typing  
✅ **Fewer errors** – no manual URLs or parameters  
✅ **Automatic authorization** – axios interceptors add tokens  
✅ **Centralized error handling** – e.g. 401 errors

## CI/CD

In CI environments, set the environment variable:
```bash
export OPENAPI_INPUT=https://your-api.com/openapi/v1.json
```
