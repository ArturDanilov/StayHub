# Azure deployment

StayHub runs as an ASP.NET Core container in Azure Container Apps and uses
Azure SQL Database. Configuration and secrets are supplied as environment
variables; production credentials must not be committed to the repository.

## Required Container App configuration

| Environment variable | Purpose |
| --- | --- |
| `ConnectionStrings__StayHubDb` | Azure SQL connection string |
| `Jwt__Issuer` | JWT issuer, for example `StayHub.Api` |
| `Jwt__Audience` | JWT audience, for example `StayHub.Mobile` |
| `Jwt__Key` | Random secret containing at least 32 characters |
| `Jwt__ExpirationMinutes` | Access-token lifetime, for example `60` |
| `SeedAdmin__Username` | Initial administrator username |
| `SeedAdmin__Email` | Initial administrator email |
| `SeedAdmin__Password` | Initial administrator password |
| `SeedData__DemoDataEnabled` | Set to `true` only for the portfolio/demo environment |
| `Swagger__Enabled` | Set to `true` for the portfolio/demo environment |

`DatabaseInitialization__ApplyMigrations` defaults to `true`. The API applies
pending EF Core migrations before accepting traffic. This is appropriate for
the current single-replica MVP. A larger deployment should run migrations as a
separate deployment step.

## Endpoints

- `/health/live` verifies that the API process is running.
- `/health/ready` verifies that the API can connect to the database.
- `/swagger` is available when `Swagger__Enabled=true`.

## Container settings

- Target port: `8080`
- Ingress: external, HTTPS
- Minimum replicas: `0`
- Maximum replicas: `1`
- CPU: `0.25`
- Memory: `0.5Gi`

The Docker image runs as the non-root user supplied by the official ASP.NET
Core image.
