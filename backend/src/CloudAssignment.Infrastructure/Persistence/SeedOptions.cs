using System.ComponentModel.DataAnnotations;

namespace CloudAssignment.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; init; }

    [MinLength(10)]
    public string DefaultPassword { get; init; } = string.Empty;
}
