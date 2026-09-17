# ADR 0002: Reservation optimistic concurrency

- Status: Accepted
- Date: 2026-09-17
- Work item: SH-8

## Context

Reservations can be changed by an Admin or Receptionist and by PMS
synchronization. The current `Reservation` model has no concurrency token.
`PUT`, status `PATCH`, and `DELETE` load the current entity and save without
knowing which version the caller originally viewed. Concurrent operations are
therefore last-write-wins and can silently overwrite each other.

The MAUI Bookings screen currently opens detail data taken from the list rather
than loading the latest reservation. Synchronization also updates tracked
reservations without detecting an intervening user or synchronization change.

## Decision

StayHub will use SQL Server `rowversion` as an EF Core optimistic-concurrency
token for `Reservation`. The value is an opaque technical version and has no
business meaning.

The HTTP API will expose that version as a strong `ETag` header on
`GET /api/reservations/{id}`. The tag value is a quoted, encoded representation
of the rowversion. Clients must not parse or compare its internal bytes; they
only store and return the complete tag.

The following mutation endpoints will require the latest tag through the
`If-Match` request header:

- `PUT /api/reservations/{id}`;
- `PATCH /api/reservations/{id}/status`;
- `DELETE /api/reservations/{id}`.

Create requests do not require a precondition. Collection responses do not
carry per-item ETags. A client must load `GET /api/reservations/{id}` before
opening a mutable detail workflow.

The API behavior will be:

| Situation | Result |
| --- | --- |
| `If-Match` is missing | `428 Precondition Required` |
| Tag is malformed | `400 Bad Request` |
| Tag does not match, the row changed during save, or the row was deleted | `412 Precondition Failed` |
| Reservation ID is not found before evaluating a supplied precondition | `412 Precondition Failed` |
| Update succeeds | Existing success response semantics |

`412` is used instead of `409` because the failure is specifically an HTTP
conditional-request failure. The response should use the existing API error
style and explain that the booking changed and must be reloaded.

EF Core must include the original rowversion in the update or delete predicate
and translate `DbUpdateConcurrencyException` into the same business-level
concurrency result. Comparing an ETag before saving is not sufficient by itself
because another writer can commit between the comparison and `SaveChanges`.

## MAUI behavior

When a booking is selected from Bookings or Today, MAUI loads the detail
endpoint and stores its ETag with the detail model. Edit, status change, and
delete requests send that value as `If-Match`.

On `412`, MAUI does not retry or silently overwrite. It shows:

> This booking changed since you opened it. Reload the latest version and try
> again. Your changes were not saved.

The user can reload the booking and consciously reapply the change. Automatic
client-side merging is outside the current scope.

## Synchronization behavior

PMS synchronization relies directly on the EF rowversion rather than HTTP
headers. A concurrency exception affects only the current external reservation,
consistent with ADR 0001. It is recorded as a failed synchronization item and
processing continues. Synchronization must not automatically overwrite a
concurrent user change or retry with new state until field ownership and PMS
source-of-truth rules are defined.

## Consequences

### Positive

- user and PMS changes cannot silently overwrite each other;
- status-transition validation cannot commit against an unnoticed stale row;
- update, status change, and delete follow one consistent concurrency model;
- ETag and conditional requests use standard HTTP semantics;
- the API does not expose SQL Server's byte-array representation in DTOs.

### Trade-offs

- a migration is required to add a non-null rowversion column;
- mutation clients must first load reservation detail and retain its ETag;
- controllers, business results, repositories, and MAUI error handling need
  explicit concurrency paths;
- a synchronization race becomes a visible per-record failure rather than an
  automatic overwrite.

## Rejected alternatives

### Keep last-write-wins

Rejected because it silently loses changes and makes user/PMS races impossible
to diagnose.

### Put a Base64 version field in every DTO

Rejected for now because the version is HTTP representation metadata rather
than a business field. ETag keeps contracts focused and uses established
conditional-request semantics.

### Use timestamps such as `UpdatedAtUtc`

Rejected because application timestamps are not guaranteed to be unique or
atomic concurrency tokens.

### Lock the reservation pessimistically while a user edits

Rejected because a mobile edit can remain open for an unbounded time and must
not hold a database transaction or lock.

### Automatically retry and overwrite after a conflict

Rejected because StayHub does not yet define whether user or PMS fields take
precedence. An automatic retry could recreate the same silent data loss this
decision prevents.

## Implementation follow-up

SH-8 records the decision only. A separate implementation work item must:

- add and configure `Reservation.RowVersion` with a migration;
- emit and validate reservation ETags;
- add concurrency results to manager and controller flows;
- update MAUI detail loading and `If-Match` handling;
- handle synchronization concurrency as a per-record failure;
- add integration tests for stale update, status change, delete, and a
  user-versus-synchronization race.
