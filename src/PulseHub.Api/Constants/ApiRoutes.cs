namespace PulseHub.Api.Constant;

public class ApiRoutes
{
    public const string Api = "api";

    public static class Sources
    {
        public const string Base = $"{Api}/sources";
        public const string ById = "{id:int}";
    }
    
}