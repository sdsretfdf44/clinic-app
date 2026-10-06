# Arrapa ClinicOS

**The Complete Clinic Operating System** — a modern, cloud-native SaaS platform by **Arrapa Software**.

Arrapa ClinicOS manages patients, appointments, consultations, prescriptions, billing, reporting, and administration
for clinics from a single platform. It is designed to scale into Hospital Management (HMS), EMR/EHR, Laboratory (LIS),
Pharmacy, Telemedicine, and Mobile.

## Status

**Phase 11 — Dashboards** ✅ (Phases 1–11 complete)

- Solution skeleton (Clean Architecture, DDD-ready, CQRS scaffolding via MediatR)
- .NET 9 / EF Core / ASP.NET Identity / JWT + refresh tokens / multi-tenant middleware
- Serilog structured logging, Swagger, API versioning, health checks, exception handling
- SQL Server + EF Core Code First (initial migration + seed)
- React 19 + Vite + TypeScript + Tailwind + TanStack Query + React Hook Form + Zod + Zustand
- Docker + docker-compose for full-stack local orchestration

## Tech stack

| Layer      | Stack                                                                                    |
|------------|-------------------------------------------------------------------------------------------|
| Backend    | .NET 9, ASP.NET Core Web API, EF Core, SQL Server, ASP.NET Identity, JWT, MediatR, FluentValidation, Mapster, Serilog, Swagger, Hangfire, Redis-ready |
| Frontend   | React 19, TypeScript, Vite, Tailwind CSS, TanStack Query, React Router, React Hook Form, Zod, Zustand |
| DevOps     | Docker, docker-compose                                                                    |

## Solution layout

```
Arrapa.ClinicOS.sln
├── src/
│   ├── Arrapa.ClinicOS.Domain           # Entities, value objects, domain events, enums
│   ├── Arrapa.ClinicOS.Application      # CQRS handlers, validators, mappings, abstractions
│   ├── Arrapa.ClinicOS.Contracts        # DTOs (public API surface)
│   ├── Arrapa.ClinicOS.Shared           # Result<T>, ApiResponse, PaginatedList, Guards
│   ├── Arrapa.ClinicOS.Persistence      # EF Core DbContext, migrations, interceptors, Identity
│   ├── Arrapa.ClinicOS.Infrastructure   # JWT, current user, tenant, clock, external services
│   ├── Arrapa.ClinicOS.API              # ASP.NET Core Web API entry point + composition
│   └── Arrapa.ClinicOS.Web              # React 19 SPA
└── tests/
    ├── Arrapa.ClinicOS.UnitTests
    └── Arrapa.ClinicOS.IntegrationTests
```

## Getting started (local dev)

### Prerequisites
- .NET 9 SDK
- Node.js 22+
- SQL Server (local instance or via docker-compose)

### 1. Backend

```bash
dotnet restore
dotnet build
dotnet ef database update --project src/Arrapa.ClinicOS.Persistence --startup-project src/Arrapa.ClinicOS.API
dotnet run --project src/Arrapa.ClinicOS.API
```

The API launches at `https://localhost:7287` (or `http://localhost:5103`). Swagger UI is at `/swagger`.
On first run the database is created and seeded with:
- Platform tenant (`arrapa`)
- Five system roles (SuperAdmin, ClinicAdmin, Doctor, Receptionist, Patient)
- Default super admin: `superadmin@arrapa.software` / `ChangeMe!2026` — **change on first login**

### 2. Frontend

```bash
cd src/Arrapa.ClinicOS.Web
npm install
npm run dev
```

Opens at `http://localhost:5173`. Vite proxies `/api/*` to the API on `http://localhost:5103`.

### 3. Full stack via Docker

```bash
docker compose up --build
```

- SQL Server → `localhost:1433`
- API      → `http://localhost:5103`
- Web      → `http://localhost:5173`

## Configuration

Backend settings live in `src/Arrapa.ClinicOS.API/appsettings.json`:

| Section                   | Purpose                                        |
|---------------------------|------------------------------------------------|
| `ConnectionStrings:Default` | SQL Server connection string                  |
| `Jwt`                     | Issuer, audience, signing key, token lifetimes |
| `Cors:AllowedOrigins`     | Frontend origins allowed to call the API       |
| `Serilog`                 | Log levels, sinks, enrichers                   |
| `Database:SeedOnStart`    | Toggles automatic migration + seed on startup  |

Never commit real secrets. Use environment variables, `dotnet user-secrets`, or a secret manager
in production. The default JWT signing key in `appsettings.json` is a placeholder and MUST be replaced.

## Multi-tenancy

Every request resolves a tenant in the following order:
1. `tid` claim on the authenticated JWT.
2. `X-Tenant-Id` header (GUID or slug).
3. Left-most subdomain label (e.g., `acme.clinicos.arrapa.com` → slug `acme`).

Every tenant-scoped entity carries a `TenantId` and is filtered by a global EF Core query filter.
Writes are stamped by the `AuditableEntitySaveChangesInterceptor` — a single, tested enforcement point for
audit trail, soft delete, and tenant isolation.

## Roadmap

| Phase | Scope                                     |
|-------|-------------------------------------------|
| 1     | Foundation & architecture (this release)  |
| 2     | Authentication module                     |
| 3     | Super Admin portal                        |
| 4     | Tenant & Clinic management                |
| 5     | Patient module                            |
| 6     | Appointment module                        |
| 7     | Doctor consultation                       |
| 8     | Reception module                          |
| 9     | Billing                                   |
| 10    | Patient portal                            |
| 11    | Dashboards                                |
| 12    | Reports                                   |
| 13    | Notifications                             |
| 14    | Audit logs                                |

## License

Proprietary — © Arrapa Software.
