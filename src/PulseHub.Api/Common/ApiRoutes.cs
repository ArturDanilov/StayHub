namespace PulseHub.Api.Common;

public class ApiRoutes
{
    private const string Api = "api";

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
}
