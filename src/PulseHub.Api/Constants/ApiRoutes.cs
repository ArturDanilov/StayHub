namespace PulseHub.Api.Constant;

public class ApiRoutes
{
    public const string Api = "api";

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
    }
}