using System.Data.Common;
using CloudAssignment.Application.Abstractions.Authentication;
using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Domain.Users;
using CloudAssignment.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CloudAssignment.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IApplicationDbContext>();
            services.RemoveAll<DbConnection>();

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var connection = serviceProvider.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
            services.AddScoped<IApplicationDbContext>(serviceProvider =>
                serviceProvider.GetRequiredService<ApplicationDbContext>());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        SeedUser(dbContext, passwordHasher, clock.UtcNow, "admin@arcana.local", "Admin Test", UserRole.Admin, null);
        SeedUser(dbContext, passwordHasher, clock.UtcNow, "teacher@arcana.local", "Teacher Test", UserRole.Teacher, null);
        SeedUser(dbContext, passwordHasher, clock.UtcNow, "student@arcana.local", "Student Test", UserRole.Student, "23DH111550");
        SeedUser(dbContext, passwordHasher, clock.UtcNow, "student2@arcana.local", "Student Two", UserRole.Student, "23DH111551");
        SeedUser(dbContext, passwordHasher, clock.UtcNow, "student3@arcana.local", "Student Three", UserRole.Student, "23DH111552");
        dbContext.SaveChanges();
        return host;
    }

    private static void SeedUser(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        DateTimeOffset utcNow,
        string email,
        string fullName,
        UserRole role,
        string? studentCode)
    {
        dbContext.Users.Add(User.Create(
            Guid.NewGuid(),
            studentCode,
            fullName,
            email,
            email.ToUpperInvariant(),
            passwordHasher.Hash("Arcana@Test2026!"),
            role,
            utcNow));
    }
}
