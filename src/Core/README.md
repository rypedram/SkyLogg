# SkyLogg Core — Clean Architecture Layers

This folder contains the **domain-centric** architecture scaffold for the Pilot Logbook system.

## Projects

| Project | Responsibility |
|---------|----------------|
| `SkyLogg.Domain` | Entities, value objects, domain services, business rules (no external dependencies) |
| `SkyLogg.Application` | CQRS commands/queries (MediatR), DTOs, validators, repository interfaces |
| `SkyLogg.Infrastructure` | Persistence adapters, OCR/AI/Map external providers, import orchestration |

## Feature Modules (Application)

```
Features/
  Flights/     Commands, Queries, DTOs, Validators, Handlers
  Aircraft/    Commands, Queries, DTOs, Validators
  Airports/    Commands, Queries, DTOs, Validators
  Import/      Commands, Queries, DTOs, Services (interfaces)
  Map/         Queries, DTOs, Services (interfaces)
```

## Integration Status

- **Wired**: `SkyLogg.Server.Api` registers `AddApplication()` + `AddInfrastructure()` in DI.
- **In-memory repositories**: Infrastructure repositories are scaffolds; next step is EF Core adapters over `AppDbContext`.
- **Existing Bit Platform code**: `Server.Api/Features/Logbook` controllers remain active during gradual migration.

## Rules

- Domain has zero framework dependencies.
- AI/OCR never writes to the database.
- Import flows: Upload → OCR → AI → Review → Confirm → Persist.
- Map layer prepares data only; rendering stays in `Client.Core`.

## Next Steps (Phase 1 MVP)

1. Replace in-memory repositories with EF-backed implementations.
2. Add MediatR endpoints or bridge existing controllers to command handlers.
3. Connect MAUI offline SQLite via shared application contracts.
4. Implement Flight wizard UI against CQRS DTOs.
