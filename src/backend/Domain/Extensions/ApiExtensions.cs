namespace Domain.Extensions
{
    public static class ApiExtensions
    {
        public const string ApiLevelClaim = "OpenAPI";

        public const string BasicApiPolicy = "BasicApiPolicy";
        public const string OpenApiPolicy = "OpenApiPolicy";

        public const string BasicApiSchemes = "Bearer";
        public const string OpenApiSchemes = "Basic,Bearer";

        public enum ApiLevel
        {
            Basic,
            Open
        }
    }
}
