# StayHub roadmap

Azure Boards is the source of truth for backlog status and work-item details.
This document records product direction and architectural sequencing without
duplicating day-to-day board management.

## Current MVP

StayHub currently provides:

- JWT authentication and role-based authorization;
- property, guest, source, user, and booking workflows;
- booking status transitions, search, filtering, sorting, and pagination;
- idempotent synchronization from a deterministic Mock PMS;
- synchronization history and a development background worker;
- a read-only AI assistant;
- a native .NET MAUI iOS client;
- SQL Server locally and Azure SQL with Azure Container Apps in production.

## Current epic: Operations Dashboard & Reliability Foundation

The current epic combines a visible operational workflow with the test and
design foundations needed before integrating a real PMS.

### Product track

- SH-0 Product terminology cleanup
- SH-1 Dashboard semantics
- SH-2 Dashboard API contract
- SH-3 Dashboard backend
- SH-4 MAUI Today screen

### Reliability track

- SH-5 Integration test foundation
- SH-6 Initial integration tests
- SH-7 Synchronization transaction-boundary ADR
- SH-8 Reservation concurrency ADR
- SH-9 CI

The MVP business timezone is `Europe/Berlin`. Partial-success synchronization
is considered desirable: one invalid external reservation should not roll back
other valid imports. Optimistic concurrency will not be implemented until its
ADR is reviewed and approved.

RabbitMQ, an outbox, SignalR, generic webhook infrastructure, scheduled Azure
jobs, OIDC, refresh tokens, and speculative multi-PMS abstractions are outside
this epic.

## Next epic

The next major epic is the first real PMS integration. The selected provider's
actual authentication, polling, pagination, rate-limit, and webhook
capabilities will determine whether StayHub needs scheduled jobs, webhooks, or
a combination. Infrastructure will be introduced only in response to those
requirements.
