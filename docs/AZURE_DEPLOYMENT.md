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
| `SeedData__DemoDataEnabled` | Keep `false` in production; enable only while initially creating demo data |
| `Swagger__Enabled` | Keep `false` in production; enable temporarily for an API demonstration |
| `AiAssistant__Enabled` | Set `true` after the Foundry model deployment is ready |
| `AiAssistant__Provider` | `AzureFoundry` in production |
| `AiAssistant__BaseAddress` | Foundry/OpenAI base URL ending in `/openai/v1/` |
| `AiAssistant__Model` | Model deployment name from Azure AI Foundry |
| `AiAssistant__ApiKey` | Foundry API key stored as a Container App secret |
| `AiAssistant__MaxOutputTokens` | Maximum tokens per answer; use `300` for the demo |
| `AiAssistant__DailyRequestLimit` | Requests per day and API replica; use `20` for the demo |

`DatabaseInitialization__ApplyMigrations` defaults to `true`. The API applies
pending EF Core migrations before accepting traffic. This is appropriate for
the current single-replica MVP. A larger deployment should run migrations as a
separate deployment step.

## Endpoints

- `/health/live` verifies that the API process is running.
- `/health/ready` verifies that the API can connect to the database.
- `/swagger` is available only when `Swagger__Enabled=true`.

## Container settings

- Target port: `8080`
- Ingress: external, HTTPS
- Minimum replicas: `0`
- Maximum replicas: `1`
- CPU: `0.25`
- Memory: `0.5Gi`

The Docker image runs as the non-root user supplied by the official ASP.NET
Core image.

## Azure AI Foundry assistant

Development continues to use local Ollama. Production can call a model
deployed in Azure AI Foundry through its OpenAI-compatible v1 endpoint.

1. In Azure AI Foundry, create a model deployment. For this MVP, choose a
   small inexpensive chat model available in the current region and assign
   the lowest practical TPM quota.
2. Copy its deployment name, endpoint, and one API key. The base address looks
   like `https://<resource>.openai.azure.com/openai/v1/` or
   `https://<resource>.services.ai.azure.com/openai/v1/`.
3. Save the key as a Container Apps secret and enable the provider:

```bash
export STAYHUB_RESOURCE_GROUP='stayhub-mvp-rg'
export STAYHUB_FOUNDRY_ENDPOINT='https://<resource>.openai.azure.com/openai/v1/'
export STAYHUB_FOUNDRY_DEPLOYMENT='<deployment-name>'
read -s STAYHUB_FOUNDRY_KEY

az containerapp secret set \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --secrets foundry-api-key="$STAYHUB_FOUNDRY_KEY"

az containerapp update \
  --name stayhub-api \
  --resource-group "$STAYHUB_RESOURCE_GROUP" \
  --set-env-vars \
    AiAssistant__Enabled=true \
    AiAssistant__Provider=AzureFoundry \
    AiAssistant__BaseAddress="$STAYHUB_FOUNDRY_ENDPOINT" \
    AiAssistant__Model="$STAYHUB_FOUNDRY_DEPLOYMENT" \
    AiAssistant__ApiKey=secretref:foundry-api-key \
    AiAssistant__TimeoutSeconds=60 \
    AiAssistant__MaxOutputTokens=300 \
    AiAssistant__DailyRequestLimit=20

unset STAYHUB_FOUNDRY_KEY
```

The API limits each response to 300 output tokens and accepts at most 20
assistant requests per day. The counter is intentionally global for this
single-replica MVP and resets when the replica restarts. Excess requests
receive HTTP 429.

These controls reduce accidental use but are not a financial hard cap. Keep
the Container App at one maximum replica, give the model deployment the lowest
practical TPM quota, and keep Azure cost alerts enabled. Azure budgets only
send notifications; they do not stop resources. A strict total-token quota is
available through Azure AI Gateway/API Management, but that extra service is
not justified for this low-cost MVP.
