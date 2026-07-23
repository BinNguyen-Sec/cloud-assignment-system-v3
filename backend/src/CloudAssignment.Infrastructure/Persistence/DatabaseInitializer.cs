using CloudAssignment.Application.Abstractions.Authentication;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAssignment.Infrastructure.Persistence;

public sealed partial class DatabaseInitializer(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IClock clock,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<SeedOptions> seedOptions,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (databaseOptions.Value.ApplyMigrationsOnStartup)
        {
            var migrations = dbContext.Database.GetMigrations();
            if (!migrations.Any())
            {
                throw new InvalidOperationException(
                    "No EF Core migration was found. Run scripts/setup-auth-database.ps1 before starting the API.");
            }

            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!seedOptions.Value.Enabled)
        {
            return;
        }

        await SeedUsersAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new SeedUser("admin@arcana.local", "Quản trị Học viện", UserRole.Admin, null),
            new SeedUser("teacher@arcana.local", "Giảng viên Arcana", UserRole.Teacher, null),
            new SeedUser("student@arcana.local", "Sinh viên Arcana", UserRole.Student, "23DH111550")
        };

        foreach (var seed in seeds)
        {
            var normalizedEmail = seed.Email.ToUpperInvariant();
            var exists = await dbContext.Users
                .AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
            if (exists)
            {
                continue;
            }

            dbContext.Users.Add(User.Create(
                Guid.NewGuid(),
                seed.StudentCode,
                seed.FullName,
                seed.Email,
                normalizedEmail,
                passwordHasher.Hash(seedOptions.Value.DefaultPassword),
                seed.Role,
                clock.UtcNow));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        LogSeedCompleted(logger);
    }

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Development seed users are ready.")]
    private static partial void LogSeedCompleted(ILogger logger);

    private sealed record SeedUser(string Email, string FullName, UserRole Role, string? StudentCode);
}
