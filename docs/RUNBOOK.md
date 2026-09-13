# StayHub operations runbook

This page is a copy-and-paste reference for running and checking StayHub.
Run commands from the repository root unless a section says otherwise.

Values in angle brackets, such as `<local-admin-password>`, are placeholders.
Never commit passwords, tokens, connection strings, or JWT keys.

## Addresses and Azure names

Set these once in every new terminal session:

```bash
cd /Users/artur.danilov/Projects/StayHub

export STAYHUB_API_LOCAL="http://localhost:5071"
export STAYHUB_MOCK_LOCAL="http://localhost:5095"
export STAYHUB_API_AZURE="https://stayhub-api.icyforest-8c1312c9.germanywestcentral.azurecontainerapps.io"
export STAYHUB_MOCK_AZURE="https://stayhub-mockpms.icyforest-8c1312c9.germanywestcentral.azurecontainerapps.io"
export STAYHUB_RESOURCE_GROUP="stayhub-mvp-rg"
export STAYHUB_CONTAINER_ENVIRONMENT="stayhub-env"
export STAYHUB_REGISTRY="ca6004515bd3acr.azurecr.io"
```

Choose which API subsequent `curl` commands use:

```bash
# Local development
export STAYHUB_API="$STAYHUB_API_LOCAL"

# Azure (use only when intentionally testing the cloud environment)
export STAYHUB_API="$STAYHUB_API_AZURE"
```

## Normal local startup

### 1. Start SQL Server

```bash
docker compose up -d
docker compose ps
```

Follow SQL Server logs if it is not healthy yet:

```bash
docker compose logs -f sqlserver
```

Configure the local seeded Admin password once per machine, or whenever it
needs to change:

```bash
dotnet user-secrets set SeedAdmin:Password '<local-admin-password>' \
  --project src/StayHub.Api
```

### 2. Start the main API

Open a second terminal:

```bash
cd /Users/artur.danilov/Projects/StayHub
dotnet run --project src/StayHub.Api
```

The API applies pending EF Core migrations and deterministic development seed
data during startup. Local Swagger is available at:

```text
http://localhost:5071/swagger
```

### 3. Start Mock PMS when testing synchronization

Open a third terminal:

```bash
cd /Users/artur.danilov/Projects/StayHub
dotnet run --project src/StayHub.MockPms
```

Verify both applications:

```bash
curl -i "$STAYHUB_API_LOCAL/health/live"
curl -i "$STAYHUB_API_LOCAL/health/ready"
curl -i "$STAYHUB_MOCK_LOCAL/health/live"
curl -sS "$STAYHUB_MOCK_LOCAL/api/reservations"
```

`/health/live` proves that the API process is running. `/health/ready` also
checks whether the main API can use its database. A live but not ready API is
usually running but cannot connect to SQL Server.

### 4. Run the MAUI application

- Select `Debug` and an iOS simulator to use the local API.
- Select `Release` and the provisioned physical iPhone to use the Azure API.
- Start `StayHub.Mobile` from Rider.

Optional command-line build checks:

```bash
dotnet build src/StayHub.Mobile/StayHub.Mobile.csproj -f net10.0-ios -c Debug
dotnet build src/StayHub.Mobile/StayHub.Mobile.csproj -f net10.0-ios -c Release
```

A physical-device build needs a valid Apple Development certificate and
provisioning profile. The installed development build can be opened without a
cable until its free Personal Team provisioning expires, normally after seven
days.

### 5. Stop local applications

Press `Ctrl+C` in the API and Mock PMS terminals. Stop SQL Server while keeping
its database volume:

```bash
docker compose down
```

Only use the following command when intentionally deleting the local database
and recreating all development data from scratch:

```bash
docker compose down -v
```

## Authentication and authorized requests

Log in against the API selected in `STAYHUB_API`:

```bash
curl -sS -X POST "$STAYHUB_API/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"<admin-password>"}'
```

Copy only the `accessToken` value from the response:

```bash
export STAYHUB_TOKEN='<access-token>'
```

Example authorized requests:

```bash
curl -sS "$STAYHUB_API/api/properties" \
  -H "Authorization: Bearer $STAYHUB_TOKEN"

curl -sS "$STAYHUB_API/api/sources" \
  -H "Authorization: Bearer $STAYHUB_TOKEN"

curl -sS "$STAYHUB_API/api/reservations" \
  -H "Authorization: Bearer $STAYHUB_TOKEN"
```

Admin, Receptionist, and Viewer have read access. Admin and Receptionist can
manage reservations and run synchronization. Only Admin can manage users or
delete entities.

## Reservation search, filtering, sorting, and pagination

```bash
curl -sS -G "$STAYHUB_API/api/reservations" \
  -H "Authorization: Bearer $STAYHUB_TOKEN" \
  --data-urlencode "search=Toni" \
  --data-urlencode "status=CheckedIn" \
  --data-urlencode "arrivalFrom=2026-07-01" \
  --data-urlencode "arrivalTo=2026-07-31" \
  --data-urlencode "propertyId=1" \
  --data-urlencode "sourceId=1" \
  --data-urlencode "sortBy=ArrivalDate" \
  --data-urlencode "sortDirection=Ascending" \
  --data-urlencode "page=1" \
  --data-urlencode "pageSize=20"
```

Allowed statuses are `Confirmed`, `CheckedIn`, `CheckedOut`, and `Cancelled`.
Allowed sort fields are `ArrivalDate`, `DepartureDate`, `GuestName`,
`PropertyName`, and `CreatedAt`. `pageSize` must be between 1 and 100.

## Manual synchronization

First find the source ID:

```bash
curl -sS "$STAYHUB_API/api/sources" \
  -H "Authorization: Bearer $STAYHUB_TOKEN"
```

Start one synchronization run and inspect recent runs:

```bash
curl -sS -X POST "$STAYHUB_API/api/synchronization/sources/<source-id>" \
  -H "Authorization: Bearer $STAYHUB_TOKEN"

curl -sS "$STAYHUB_API/api/synchronization/runs?take=20" \
  -H "Authorization: Bearer $STAYHUB_TOKEN"
```

For fully local synchronization, both the main API on port `5071` and Mock PMS
on port `5095` must be running. Production automatic synchronization is
disabled, so Azure resources are used only when a user or API client explicitly
starts a run.

## Endpoint reference

All endpoints except login and health checks require a bearer token.

| Method | Endpoint | Access | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Public | Create a JWT access token |
| `GET` | `/health/live` | Public | Check whether the API process is alive |
| `GET` | `/health/ready` | Public | Check API and database readiness |
| `GET` | `/api/properties` | Admin, Receptionist, Viewer | List properties |
| `GET` | `/api/properties/{id}` | Admin, Receptionist, Viewer | Get one property |
| `POST` | `/api/properties` | Admin, Receptionist | Create a property |
| `PUT` | `/api/properties/{id}` | Admin, Receptionist | Update a property |
| `DELETE` | `/api/properties/{id}` | Admin | Delete a property |
| `GET` | `/api/guests` | Admin, Receptionist, Viewer | List guests |
| `GET` | `/api/guests/{id}` | Admin, Receptionist, Viewer | Get one guest |
| `POST` | `/api/guests` | Admin, Receptionist | Create a guest |
| `PUT` | `/api/guests/{id}` | Admin, Receptionist | Update a guest |
| `DELETE` | `/api/guests/{id}` | Admin | Delete a guest |
| `GET` | `/api/sources` | Admin, Receptionist, Viewer | List reservation sources |
| `GET` | `/api/sources/{id}` | Admin, Receptionist, Viewer | Get one source |
| `POST` | `/api/sources` | Admin, Receptionist | Create a source |
| `PUT` | `/api/sources/{id}` | Admin, Receptionist | Update a source |
| `DELETE` | `/api/sources/{id}` | Admin | Delete a source |
| `GET` | `/api/reservations` | Admin, Receptionist, Viewer | Search and page reservations |
| `GET` | `/api/reservations/{id}` | Admin, Receptionist, Viewer | Get reservation details |
| `POST` | `/api/reservations` | Admin, Receptionist | Create a reservation |
| `PUT` | `/api/reservations/{id}` | Admin, Receptionist | Update a reservation |
| `PATCH` | `/api/reservations/{id}/status` | Admin, Receptionist | Change reservation status |
| `DELETE` | `/api/reservations/{id}` | Admin | Delete a reservation |
| `GET` | `/api/users` | Admin | List users |
| `POST` | `/api/users` | Admin | Create a user |
| `PATCH` | `/api/users/{id}/role` | Admin | Change a role |
| `PATCH` | `/api/users/{id}/status` | Admin | Activate or deactivate a user |
| `PUT` | `/api/users/{id}/password` | Admin | Reset a user password |
| `POST` | `/api/synchronization/sources/{sourceId}` | Admin, Receptionist | Run source synchronization |
| `GET` | `/api/synchronization/runs?take=20` | Admin, Receptionist | Read recent synchronization runs |

Mock PMS has two public development endpoints:

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/reservations` | Return deterministic external reservations |
| `GET` | `/health/live` | Check whether Mock PMS is alive |

Use local Swagger to inspect request bodies and execute endpoints interactively.

## Database commands

Database migrations normally run automatically when the development API
starts. These commands are useful for diagnosis or an explicit manual update:

```bash
dotnet ef migrations list \
  --project src/StayHub.Dal \
  --startup-project src/StayHub.Api

dotnet ef database update \
  --project src/StayHub.Dal \
  --startup-project src/StayHub.Api
```

Check local listeners when an application cannot connect:

```bash
lsof -nP -iTCP:1433 -sTCP:LISTEN
lsof -nP -iTCP:5071 -sTCP:LISTEN
lsof -nP -iTCP:5095 -sTCP:LISTEN
```

## Build and tests

```bash
dotnet build src/StayHub.Api/StayHub.Api.csproj
dotnet run --project tests/StayHub.Business.Tests
```

The business test project is an executable xUnit v3 test project, so
`dotnet run` executes its tests directly.

## Azure operations

Authenticate and select the subscription:

```bash
az login
az account set --subscription "Azure subscription 1"
az account show --output table
```

List the applications and inspect their current images:

```bash
az containerapp list \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --output table

az containerapp show \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --query properties.template.containers[0].image \
  --output tsv

az containerapp show \
  --name stayhub-mockpms \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --query properties.template.containers[0].image \
  --output tsv
```

Check the cloud applications. With `minReplicas=0`, the first request can take
longer because it wakes a scaled-to-zero application:

```bash
curl -i "$STAYHUB_API_AZURE/health/live"
curl -i "$STAYHUB_API_AZURE/health/ready"
curl -i "$STAYHUB_MOCK_AZURE/health/live"
curl -sS "$STAYHUB_MOCK_AZURE/api/reservations"
```

Follow logs:

```bash
az containerapp logs show \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --follow \
  --tail 100

az containerapp logs show \
  --name stayhub-mockpms \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --follow \
  --tail 100
```

List revisions and verify scale limits:

```bash
az containerapp revision list \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --output table

az containerapp show \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --query '{minReplicas:properties.template.scale.minReplicas,maxReplicas:properties.template.scale.maxReplicas}' \
  --output table
```

Container Apps do not need a manual start command in this configuration. An
incoming request wakes an application with `minReplicas=0`. To restart a
specific active revision during diagnosis:

```bash
export STAYHUB_API_REVISION='<active-revision-name>'

az containerapp revision restart \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --revision "$STAYHUB_API_REVISION"
```

### Deploy a new main API image

Deploy only when cloud behavior must change; local development does not require
an Azure deployment.

```bash
az acr login --name ca6004515bd3acr
export STAYHUB_IMAGE_TAG="$(date +%Y%m%d-%H%M)"

docker buildx build \
  --platform linux/amd64 \
  --file Dockerfile \
  --tag "$STAYHUB_REGISTRY/stayhub-api:$STAYHUB_IMAGE_TAG" \
  --push \
  .

az containerapp update \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --image "$STAYHUB_REGISTRY/stayhub-api:$STAYHUB_IMAGE_TAG"
```

### Deploy a new Mock PMS image

```bash
az acr login --name ca6004515bd3acr
export STAYHUB_MOCK_IMAGE_TAG="$(date +%Y%m%d-%H%M)"

docker buildx build \
  --platform linux/amd64 \
  --file Dockerfile.MockPms \
  --tag "$STAYHUB_REGISTRY/stayhub-mockpms:$STAYHUB_MOCK_IMAGE_TAG" \
  --push \
  .

az containerapp update \
  --name stayhub-mockpms \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --image "$STAYHUB_REGISTRY/stayhub-mockpms:$STAYHUB_MOCK_IMAGE_TAG"
```

After any deployment, run the Azure health checks above and inspect the active
revision and logs. Production secrets remain configured on the Container App;
they must never be included in an image command or committed to Git.
