# UI terminology

StayHub uses hospitality-friendly terms in the user interface while preserving
precise and stable names in the domain model and API.

| UI term | Meaning in StayHub | Internal name |
| --- | --- | --- |
| Booking | A guest's booked stay at a property | `Reservation` |
| Guest | The person associated with a booking | `Guest` |
| Property | A hotel or other accommodation | `Property` |
| Check-in | The action that moves a confirmed booking to in-house | `CheckedIn` |
| Check-out | The action that completes an in-house booking | `CheckedOut` |
| Synchronization | Importing booking data from an external PMS | Synchronization |
| AI | The read-only StayHub assistant | Assistant |

Use **Booking** in visible mobile UI text. Keep **Reservation** in C# domain
types, API routes, contracts, database objects, and technical documentation
where the implementation is being described. A provider may call the same
concept a reservation or a stay; provider terminology is translated to the
StayHub domain at the integration boundary.

Use **Check in** and **Check out** for actions. Use **Checked in** and
**Checked out** for states. Use **Synchronization** in headings and explanatory
text; the shorter **Sync** is acceptable for a compact navigation label or
button.
