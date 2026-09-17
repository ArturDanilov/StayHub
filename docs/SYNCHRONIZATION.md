# Reservation synchronization

StayHub can import reservations from an external PMS over HTTP. The first
integration is `StayHub.MockPms`, a deterministic local API that represents a
provider such as Booking.com, Airbnb, Apaleo, or Mews without requiring a real
provider account.

## Local demonstration

Start SQL Server, the main API, and Mock PMS in separate terminals:

```bash
docker compose up -d
dotnet run --project src/StayHub.Api
dotnet run --project src/StayHub.MockPms
```

The seeded `Mock PMS` source points to `http://localhost:5095`. Sign in as an
Admin or Receptionist and open the **Sync** tab in the MAUI app. Select Mock PMS
and press **Sync now**.

The equivalent API calls are:

```text
POST /api/synchronization/sources/{sourceId}
GET  /api/synchronization/runs?take=20
```

Both endpoints require the `ManageReservations` authorization policy.

## Import rules

- `(SourceId, ExternalId)` is the stable identity of an imported reservation.
- A repeated import updates the existing reservation rather than creating a
  duplicate.
- Guests are matched by normalized email address.
- Properties are matched by their exact trimmed name.
- Overlapping active reservations for the same property are counted as
  conflicts in the synchronization result.
- Invalid individual records are skipped and recorded as failures, while valid
  records in the same run continue importing.
- Every run records its status, timestamps, created/updated/unchanged counts,
  conflict count, failure count, and a bounded error summary.

The accepted transaction behavior is documented in
[ADR 0001](adr/0001-synchronization-transaction-boundaries.md). One external
reservation is the atomic unit; the complete run is intentionally not wrapped
in one transaction. SH-7 records this design decision and does not yet change
the current persistence implementation.

Reservation updates will use optimistic concurrency as defined in
[ADR 0002](adr/0002-reservation-optimistic-concurrency.md). A user/PMS race will
be reported for that external reservation instead of silently overwriting the
other change. SH-8 records this decision and does not yet add the concurrency
token or migration.

## Cloud note

`localhost` only works when both APIs run on the same development machine. For
Azure, deploy `StayHub.MockPms` separately and update the Mock PMS source URL to
its HTTPS endpoint.

## Automatic synchronization

The API contains a background worker that can periodically synchronize enabled
sources with a configured URL. It uses the same `SynchronizationManager` as the
manual endpoint, so import rules and run history stay identical. A per-source
execution gate prevents a manual and an automatic run from executing at the
same time inside one API instance.

Development enables the worker only for `Mock PMS`: it waits 30 seconds after
API startup and then runs at most once every six hours. Production disables the
worker by default so Azure Container Apps can scale to zero without a permanent
replica consuming resources. Configuration is controlled through:

```json
{
  "AutomaticSynchronization": {
    "Enabled": false,
    "IntervalMinutes": 720,
    "InitialDelaySeconds": 60,
    "SourceNames": []
  }
}
```

For a low-traffic Azure deployment, prefer a scheduled Container Apps Job when
automatic cloud synchronization is required. A continuously hosted worker
cannot run while the API is scaled to zero.
