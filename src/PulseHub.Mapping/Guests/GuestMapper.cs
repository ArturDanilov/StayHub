using PulseHub.Contracts.Guests;
using PulseHub.Domain.Models;

namespace PulseHub.Mapping.Guests;

public static class GuestMapper
{
    public static Guest ToDomain(CreateGuestRequest request)
    {
        return new Guest
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static void MapToDomain(
        UpdateGuestRequest request,
        Guest guest)
    {
        guest.FirstName = request.FirstName.Trim();
        guest.LastName = request.LastName.Trim();
        guest.Email = request.Email.Trim().ToLowerInvariant();
        guest.Phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : request.Phone.Trim();
    }

    public static GuestResponse ToResponse(Guest guest)
    {
        return new GuestResponse(
            guest.Id,
            guest.FirstName,
            guest.LastName,
            guest.Email,
            guest.Phone,
            guest.CreatedAtUtc);
    }
}
