using System.ComponentModel.DataAnnotations;

namespace CloudAssignment.Api.Configuration;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public const string PolicyName = "FrontendPolicy";

    [Required]
    [MinLength(1)]
    public string[] AllowedOrigins { get; init; } = ["http://localhost:5173"];
}
