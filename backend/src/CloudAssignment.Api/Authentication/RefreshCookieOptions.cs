using System.ComponentModel.DataAnnotations;

namespace CloudAssignment.Api.Authentication;

public sealed class RefreshCookieOptions
{
    public const string SectionName = "RefreshCookie";

    [Required]
    public string Name { get; init; } = "cloud_assignment_refresh";

    public bool Secure { get; init; }

    [Required]
    public string SameSite { get; init; } = "Lax";
}
