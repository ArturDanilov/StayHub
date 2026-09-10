using System.Reflection;
using System.Text.Json;

namespace StayHub.Mobile.Configuration;

public sealed class ApiSettings
{
    private const string ResourceName = "StayHub.Mobile.Configuration.appsettings.json";

    public string BaseAddress { get; init; } = string.Empty;

    public static ApiSettings Load()
    {
        using var stream = Assembly
                               .GetExecutingAssembly()
                               .GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded configuration '{ResourceName}' was not found.");

        var settings = JsonSerializer.Deserialize<ApiSettings>(stream)
                       ?? throw new InvalidOperationException(
                           "The mobile API configuration is invalid.");

        if (!Uri.TryCreate(settings.BaseAddress, UriKind.Absolute, out var baseAddress)
            || (baseAddress.Scheme != Uri.UriSchemeHttp
                && baseAddress.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "The configured API base address must be an absolute HTTP or HTTPS URL.");
        }

        return settings;
    }
}
