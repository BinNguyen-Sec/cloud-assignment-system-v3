using CloudAssignment.Application.Features.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace CloudAssignment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        return services;
    }
}
