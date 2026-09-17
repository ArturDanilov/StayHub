# ADR 0001: Synchronization transaction boundaries

- Status: Accepted
- Date: 2026-09-17
- Work item: SH-7

## Context

StayHub deliberately supports partial-success synchronization. One invalid or
unpersistable external reservation must not roll back reservations that were
already imported successfully in the same run.

The current implementation uses one scoped `StayHubDbContext` for the complete
run. Repository `AddAsync` methods call `SaveChangesAsync` independently. This
creates two correctness risks:

- a newly created guest is committed before its reservation, so a later
  reservation failure can leave an unintended guest row;
- after a failed `SaveChangesAsync`, failed tracked changes remain in the shared
  context and can be retried accidentally when a later reservation saves.

The synchronization run itself is created before the external PMS call and is
updated after processing. HTTP calls must not hold database transactions open.

## Decision

The atomic business unit is one external reservation.

Each external reservation will be processed with a fresh `StayHubDbContext`
and one explicit SQL transaction using the default `ReadCommitted` isolation
level. The transaction contains:

1. property lookup;
2. guest lookup and optional creation;
3. existing reservation lookup by `(SourceId, ExternalId)`;
4. conflict detection;
5. reservation creation or update;
6. the synchronization-run counter for that successful outcome;
7. one `SaveChangesAsync` followed by transaction commit.

Guest and reservation changes therefore commit together. A rollback discards
the entire external-reservation unit, including tracked state, because that
unit's context is disposed after the attempt.

There is no transaction around the complete synchronization run. The external
HTTP request is completed before per-reservation transactions begin. Records
are processed sequentially for now.

If a reservation attempt fails, its transaction is rolled back. A clean,
short-lived context then records `FailedCount` and the bounded error summary on
the synchronization run. Processing continues with the next external record.
The final run status is:

- `Completed` when all records were processed without failure;
- `CompletedWithErrors` when at least one record failed but the run continued;
- `Failed` when the PMS request or another run-level operation failed.

Created, updated, unchanged, conflict, and failure counters are persisted as
records are processed rather than only at the end. The final status and
`CompletedAtUtc` are saved in a separate short transaction. This leaves useful
progress information if the process stops unexpectedly, while an interrupted
run can remain `Running` until a future recovery policy is introduced.

The implementation should expose a synchronization-specific unit-of-work or
record processor from the DAL boundary. Business code should not depend
directly on EF Core transaction types. Because SQL retry-on-failure is enabled,
an explicit transaction must execute through EF Core's configured execution
strategy.

## Consequences

### Positive

- one bad reservation does not undo successful imports;
- guest and reservation creation become atomic;
- failed tracked entities cannot poison later records;
- run metrics reflect committed work more accurately;
- transactions remain short and never include PMS network latency.

### Trade-offs

- a run performs more short transactions and creates more DbContext scopes;
- run counters are updated more frequently;
- the implementation requires a focused synchronization persistence boundary
  instead of composing repository methods that save independently;
- multiple application replicas still need a distributed execution guard in
  the future; the current in-memory per-source gate is outside this decision.

## Rejected alternatives

### One transaction for the complete run

Rejected because one invalid record would roll back every successful record,
transactions could remain open for a long time, and partial success is an
explicit product requirement.

### Keep the shared DbContext and rely on separate `SaveChangesAsync` calls

Rejected because those calls do not make guest and reservation creation atomic
and a failed change tracker can affect later records.

### One transaction per repository method

Rejected because repository-method boundaries do not match the business unit.
Guest creation and reservation persistence must succeed or fail together.

## Implementation follow-up

SH-7 records the decision only. A separate implementation work item must:

- introduce the per-reservation persistence boundary;
- remove intermediate synchronization-path `SaveChangesAsync` calls;
- persist outcome counters according to this ADR;
- add integration tests proving rollback of guest plus reservation and
  continuation after a failed record.

No database schema migration is required for the transaction boundary itself.
