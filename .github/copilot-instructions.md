# Copilot Instructions for Garagge

## Architecture Overview
Garagge is a vehicle management application with **Clean Architecture** and strict layer separation. Backend uses .NET with CQRS/MediatR, frontend is Vue.js 3 + Vuetify with auto-generated API client.

### Layer Structure (Backend)
- **Domain** (`server/src/Domain/`): Entities, enums, domain events. No dependencies on other layers
- **Application** (`server/src/Application/`): CQRS handlers, business logic. Uses MediatR, Result pattern
- **Infrastructure** (`server/src/Infrastructure/`): Data access, external services. EF Core with PostgreSQL
- **Api** (`server/src/Api/`): Minimal API endpoints. Maps to MediatR commands/queries

**Critical**: Respect dependency rules enforced by `ArchitectureTests`. Domain cannot reference other layers, Application cannot reference Infrastructure/Api.

## Key Patterns

### CQRS with Result Pattern
Commands and queries are separate with explicit error handling:
```csharp
// Command pattern
public record CreateMyVehicleCommand(string Brand, string Model, PowerType PowerType) : ICommand<VehicleDto>;

// Handler returns Result<T> - never throw exceptions
public async Task<Result<VehicleDto>> Handle(CreateMyVehicleCommand request, CancellationToken cancellationToken)
{
    if (userId == Guid.Empty)
        return Result.Failure<VehicleDto>(VehicleErrors.Unauthorized);
    // ...
}
```

### Minimal API Endpoints
Endpoints are classes implementing `IEndpoint` in `server/src/Api/Endpoints/`:
```csharp
public class GetMyVehicles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("vehicles/my", async (ISender sender, ClaimsPrincipal user) => {
            var query = new GetMyVehiclesQuery(user.GetUserId());
            Result<PagedList<VehicleDto>> result = await sender.Send(query);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.UsersAccess)
        .WithTags(Tags.Vehicles);
    }
}
```

### Domain Events & Entity Base Class
All entities inherit from `Entity` with built-in domain events:
```csharp
public abstract class Entity
{
    public void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    // Events published automatically in ApplicationDbContext.SaveChangesAsync
}
```

## Frontend Patterns

### Auto-Generated API Client
Use Orval to generate TypeScript client from OpenAPI spec:
- **Generate**: `npm run generate:api` (from `web/`)  
- **Generated files**: `src/api/generated/` (DO NOT edit manually)
- **Custom axios instance**: `src/api/axios-instance.ts` handles auth tokens + 401 redirects

### Pinia Store Pattern
State management with Pinia. Example `userStore.ts`:
```typescript
export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    accessToken: localStorage.getItem(TOKEN_STORAGE_KEY) || '',
  }),
  actions: {
    setToken(accessToken: string) {
      // Always sync with localStorage
    }
  }
})
```

### Route Guards
Authentication handled in `router/index.ts`:
```typescript
router.beforeEach((to, from, next) => {
  const userStore = useUserStore()
  if (to.meta.requiresAuth && !userStore.accessToken) {
    next('/login')
  }
})
```

## Development Workflows

### Docker Development Modes
- **Frontend dev**: `docker-compose up server db seq` + `cd web && npm run dev`
- **Backend dev**: `docker-compose up db seq web` + run API in IDE
- **Full stack**: `docker-compose up db seq` + run both locally

### Testing Patterns
- **Unit tests**: Use `InMemoryDbTestBase` for Application layer handlers
- **Integration tests**: Test endpoints with real database in `ApiIntegrationTests`
- **Architecture tests**: Verify layer dependencies with NetArchTest

### API Client Regeneration
When backend OpenAPI changes:
1. Ensure backend is running (`dotnet run` from `server/src/Api/`)
2. From `web/`: `npm run generate:api`
3. Commit generated files - they're part of the codebase

## Project-Specific Conventions

### Error Handling
- Backend: Return `Result<T>` - never throw in handlers
- Frontend: Axios interceptors handle 401s globally, component-level error states for others

### Entity Configuration
EF Core configurations in `Infrastructure/DAL/Configurations/`. Use Table Per Hierarchy for inheritance (e.g., `EnergyEntry` → `FuelEntry`/`ChargingEntry`)

### Permissions
Permission-based authorization with `HasPermission(Permissions.UsersAccess)` on endpoints. Permissions defined in `Api/Endpoints/Users/Permissions.cs`

### Database Migrations
- Always use snake_case naming convention (configured in `ApplicationDbContext`)
- Seed data in `DatabaseSeeder.cs`
- Apply with `app.ApplyMigrations()` in `Program.cs`

## Key Files to Reference
- `server/src/Application/DependencyInjection.cs` - MediatR + validation pipeline setup
- `web/src/api/axios-instance.ts` - Auth token handling
- `server/src/Api/Program.cs` - Application bootstrap + middleware pipeline
- `web/orval.config.ts` - API client generation config
