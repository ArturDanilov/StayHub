namespace StayHub.Business.Results;

public sealed class AssistantUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
