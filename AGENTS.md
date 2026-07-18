# StayHub Repository Guidance

## Project Purpose

StayHub is a long-term personal learning project for developing production-oriented backend engineering skills and preparing for mid-level and senior .NET interviews in Germany, particularly Munich.

This is not a tutorial CRUD application. Build features that resemble a real production system while keeping the project manageable for a single developer. Prefer quality over quantity, and make every architectural decision for a concrete reason.

StayHub is an API-first hospitality integration platform. It stores properties, guests, reservations, and their external sources. Its long-term purpose is to synchronize reservation data from systems such as Booking.com, Apaleo, Mews, Expedia, and a Mock PMS. It is inspired by hospitality platforms such as Apaleo, but it is not intended to clone them.

If another repository instruction describes StayHub as a telemetry platform, treat that description as outdated. The hospitality domain described here and in `docs/ARCHITECTURE.md` is authoritative.

## Technology and Structure

The current stack is:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server in Docker
- JWT authentication
- Role-based authorization
- Swagger
- Manual mapping
- Repository pattern
- Manager/service layer
- Clean layered architecture

The solution is organized into:

- `StayHub.Api`: controllers, authentication, dependency injection, and API configuration
- `StayHub.Business`: business logic, managers, validation, repository interfaces, and operation results
- `StayHub.Contracts`: request and response DTOs
- `StayHub.Dal`: EF Core, repositories, migrations, and database access
- `StayHub.Domain`: domain entities and enums
- `StayHub.Mapping`: explicit mapping between domain models and contracts

Keep dependency direction and layer responsibilities clear. Controllers should work with contracts rather than expose EF Core entities. Business rules belong in managers. Repositories should focus on persistence.

## Current Domain

The core entities are:

- `Property`: a hotel or other accommodation
- `Guest`: a customer who may have multiple reservations
- `Reservation`: a booking associated with a guest and property
- `Source`: the external provider or import mechanism from which reservation data originates
- `ReservationStatus`: the reservation lifecycle state

A reservation currently includes an external identifier, guest, property, arrival and departure dates, and status. Sources include providers or mechanisms such as Booking.com, Apaleo, manual import, and a Mock PMS. A source describes an integration; it does not perform synchronization itself.

## Engineering Principles

- Prefer readable, explicit, maintainable code.
- Use small methods and precise names.
- Explain why a design is better, not only how it works.
- Prefer manual mapping over AutoMapper.
- Keep repositories and managers explicit.
- Avoid hidden behavior and unnecessary magic.
- Follow existing conventions unless there is a concrete reason to improve them.
- Choose the implementation another developer can understand most easily when multiple options are equally valid.
- Add abstractions only when they solve a current problem or remove meaningful duplication.
- Prefer realistic business logic over adding more technologies.
- Ask of every feature: "Would this exist in a real production system?"

Do not introduce complexity merely because it is technically possible. Avoid proposing microservices, CQRS, event sourcing, MediatR everywhere, RabbitMQ, Redis, Kubernetes, pervasive DDD aggregates, or generic repository abstractions unless an actual project requirement justifies them.

## Backend Topics Worth Developing

Prioritize practical backend engineering topics when they naturally fit the project:

- EF Core best practices and migrations
- SQL schema design and indexes
- transactions and optimistic concurrency
- background services
- `HttpClient` usage and resilience
- retries with appropriate boundaries
- idempotent synchronization
- integration testing
- authentication and authorization
- API versioning
- structured logging
- validation
- Docker-based development
- caching only when a demonstrated need exists

A well-designed reservation synchronization pipeline is more valuable here than introducing infrastructure without a concrete use case.

## Roadmap Direction

Expected future work includes:

- reservation synchronization
- a `BackgroundService` synchronization worker
- Mock PMS integration
- Apaleo sandbox integration
- SignalR notifications
- reservation filtering, pagination, and search
- webhooks
- idempotent synchronization
- integration tests

Let the architecture evolve from these requirements. Do not build speculative infrastructure for roadmap items before it is needed.

## Database and Seed Data

Development uses a disposable SQL Server database running in Docker. Development data should be seeded automatically.

Seed data must be deterministic, realistic, and useful for demonstrations. Prefer meaningful scenarios over random values, including:

- multiple properties
- multiple guests
- reservations in different statuses
- past, current, and future reservations

The seeded dataset should make Swagger useful for demonstrating filtering, searching, validation, and business rules.

## Review and Change Expectations

When reviewing or changing code:

- preserve simplicity and existing architectural boundaries;
- identify hidden correctness, data integrity, security, and concurrency problems;
- distinguish necessary changes from optional polish;
- recommend improvements only when they provide concrete value;
- explain trade-offs and the reason behind important suggestions;
- keep changes focused and avoid unrelated refactoring;
- add or update tests in proportion to the behavior and risk;
- optimize for maintainability and interview-quality code, not maximum architectural complexity.

Before considering a change complete, run the most relevant available build and tests. Do not modify migrations, generated files, public API contracts, or architecture-wide conventions incidentally.
