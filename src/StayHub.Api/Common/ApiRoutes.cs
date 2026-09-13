namespace StayHub.Api.Common;

public class ApiRoutes
{
    private const string Api = "api";

    public static class Assistant
    {
        public const string Base = $"{Api}/assistant";
        public const string Chat = "chat";
    }

    public static class Authentication
    {
        public const string Base = $"{Api}/auth";
        public const string Login = "login";
    }
    
    public static class Sources
    {
        public const string Base = $"{Api}/sources";
        public const string ById = "{id:int}";
    }
    
    public static class Properties
    {
        public const string Base = $"{Api}/properties";
        public const string ById = "{id:int}";
    }

    public static class Reservations
    {
        public const string Base = $"{Api}/reservations";
        public const string ById = "{id:int}";
        public const string Status = "{id:int}/status";
    }
    
    public static class Guests
    {
        public const string Base = $"{Api}/guests";
        public const string ById = "{id:int}";
    }

    public static class Users
    {
        public const string Base = $"{Api}/users";
        public const string Role = "{id:int}/role";
        public const string Status = "{id:int}/status";
        public const string Password = "{id:int}/password";
    }

    public static class Synchronization
    {
        public const string Base = $"{Api}/synchronization";
        public const string Source = "sources/{sourceId:int}";
        public const string Runs = "runs";
    }
}
