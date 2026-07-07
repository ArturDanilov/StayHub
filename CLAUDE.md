# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

PulseHub is a telemetry platform for collecting, normalizing, storing and visualizing data from
heterogeneous sources (mock sensors, weather APIs, smart home devices, later MQTT/Home Assistant).
It is an early-stage .NET 10 modular monolith — most projects are still empty scaffolding
(`Class1.cs` stubs). See `wiki/ARCHITECTURE.md` for the full design doc; the key points are
repeated below since they drive naming and module boundaries throughout the codebase.

### Core data flow

```
Source -> Collector -> Parser -> Normalizer -> Measurement -> Database -> API -> SignalR -> UI
```

- **Source**: describes where data comes from (mock sensor, weather API, Shelly device, MQTT
  topic, Home Assistant, virtual PLC). Stores no data itself, just describes the origin.
- **Collector**: knows how to fetch raw data from a specific Source (HTTP, MQTT, mock, file).
- **Parser**: understands the raw data format (JSON, plain text, source-specific parsers) and
  turns it into structured intermediate data.
- **Normalizer**: converts parsed data into a unified internal format (MetricName, Value, Unit,
  Timestamp, SourceId), since different sources use different field names/units.
- **Measurement**: the normalized, stored telemetry value.

### Architectural principles (from wiki/ARCHITECTURE.md)

- Stay a modular monolith until clear module boundaries emerge — don't jump to microservices early.
- Keep external/raw data separate from internal domain data; always parse+normalize before it
  becomes a Measurement.
- Avoid introducing abstractions/patterns that don't solve a real, current problem.
- First MVP scope is intentionally narrow: create a Source, generate mock measurements, store in
  SQL Server, read latest/history via API. No auth, no frontend, no microservices yet.

## Solution layout

`PulseHub.sln` wires together these projects (dependencies flow downward):

```
PulseHub.Api        ASP.NET Core Web API (controllers, DI, Swagger). Depends on Dal.
PulseHub.Business   Business/domain logic (currently empty stub).
PulseHub.Contracts  DTOs/contracts shared across layers (currently empty stub).
PulseHub.Mapping    Mapping between Domain models and Contracts (currently empty stub). Depends on Domain, Contracts.
PulseHub.Domain     Domain models (e.g. Source). No dependencies.
PulseHub.Dal        EF Core DbContext + migrations, SQL Server. Depends on Domain.
```

Note the current API controllers (e.g. `SourcesController`) talk to `PulseHubDbContext` directly
and return `PulseHub.Domain.Models` types straight over the wire — the Business/Contracts/Mapping
layers exist in the solution but aren't wired in yet. When adding features, prefer following
existing conventions in the repo over what the layer names imply until this settles.

All projects target `net10.0` with `Nullable` and `ImplicitUsings` enabled.

## Commands

Requires .NET 10 SDK (`dotnet --version` should report `10.x`).

```bash
# Start SQL Server (required for the API/EF Core to work)
docker compose up -d

# Restore, build, run
dotnet restore
dotnet build
dotnet run --project src/PulseHub.Api          # serves API + Swagger UI at /swagger in Development

# EF Core migrations (run from repo root, targeting the Dal project, startup project is Api)
dotnet ef migrations add <Name> --project src/PulseHub.Dal --startup-project src/PulseHub.Api
dotnet ef database update --project src/PulseHub.Dal --startup-project src/PulseHub.Api

# Tests
dotnet test
```

The `tests/` directory currently exists but is empty — no test project has been created yet.

## Local database

`docker-compose.yml` runs SQL Server 2022 on `localhost:1433` (container `pulsehub-sqlserver`).
Credentials and the `PulseHubDb` connection string are in
`src/PulseHub.Api/appsettings.Development.json` (dev-only, matches the docker-compose password).
