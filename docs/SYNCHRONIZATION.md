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

## Cloud note

`localhost` only works when both APIs run on the same development machine. For
Azure, deploy `StayHub.MockPms` separately and update the Mock PMS source URL to
its HTTPS endpoint. A scheduled background worker is intentionally postponed
until manual synchronization is proven reliable.
