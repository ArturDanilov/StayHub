# Operations dashboard

The Today dashboard is the operational landing page for StayHub users. It
presents a small preview of the bookings that need attention without replacing
the complete Bookings workflow.

## Business date

The MVP uses `Europe/Berlin` as its single business timezone. The backend
derives `BusinessDate` by converting the current instant to this timezone and
then taking its calendar date. It must not derive the business date directly
from UTC or from the mobile device clock.

Property-specific timezones are outside the MVP. The fixed timezone is an
explicit limitation that can be revisited when StayHub supports properties in
multiple operational timezones.

## Dashboard sections

Given the inclusive current `BusinessDate`:

| Section | Inclusion rule |
| --- | --- |
| Today's arrivals | `ArrivalDate == BusinessDate` and status is not `Cancelled` |
| Today's departures | `DepartureDate == BusinessDate` and status is not `Cancelled` |
| In house | Status is `CheckedIn` |
| Upcoming arrivals | `ArrivalDate > BusinessDate`, `ArrivalDate <= BusinessDate + 7 days`, and status is `Confirmed` |
| Synchronization alerts | Run is `Failed` or `CompletedWithErrors`, or has `ConflictCount > 0` or `FailedCount > 0` |

Today's arrivals are not repeated in Upcoming arrivals. In-house membership is
based on the explicit `CheckedIn` status rather than the date range because a
guest is not operationally in house until check-in has occurred.

## Ordering and limits

Each booking section exposes the full `TotalCount` and at most five preview
items. Preview items use deterministic ordering:

- Today's arrivals: arrival date, guest last name, guest first name, booking ID.
- Today's departures: departure date, guest last name, guest first name,
  booking ID.
- In house: departure date, guest last name, guest first name, booking ID.
- Upcoming arrivals: arrival date, guest last name, guest first name, booking ID.

Synchronization alerts show the five most recently started matching runs,
ordered by `StartedAtUtc` descending and then by run ID descending. An alert may
match more than one rule but appears only once.

## API contract

The dedicated dashboard response contains:

- `BusinessDate` calculated by the backend;
- `Arrivals`, `Departures`, `InHouse`, and `UpcomingArrivals` sections;
- `SynchronizationAlerts` containing at most five recent alerts.

Each booking section has `TotalCount` for the complete matching result and
`Items` containing at most five preview records. A booking preview contains the
booking ID used for navigation, dates, status, property identity and name, and
guest identity and name. It intentionally does not duplicate the complete
reservation response.

A synchronization alert contains the run and source identity, status, start
time, conflict and failure counts, and an optional error message. The contract
does not expose synchronization implementation details.

The dashboard is available through `GET /api/operations-dashboard`. Its EF Core
queries are read-only, filter and project on the database side, and return only
the limited preview records defined above.

## Access and API boundary

Admin, Receptionist, and Viewer roles can read the dashboard through the
existing read-access policy. The dashboard will use a dedicated read-oriented
endpoint rather than requiring the mobile client to compose several generic
API calls.
