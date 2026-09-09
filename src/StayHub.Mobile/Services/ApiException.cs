namespace StayHub.Mobile.Services;

public sealed class ApiException(string message) : Exception(message);
