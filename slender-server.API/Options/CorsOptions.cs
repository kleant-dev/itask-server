namespace slender_server.API.Options;

public sealed class CorsOptions
{
    public const string PolicyName = "SlenderCorsPolicy";
    public const string SectionName = "Cors";

    public required string[] AllowedOrigins { get; init; }
}