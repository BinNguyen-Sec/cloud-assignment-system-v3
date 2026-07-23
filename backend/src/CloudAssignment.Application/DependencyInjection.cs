using CloudAssignment.Application.Features.Auth;
using CloudAssignment.Application.Features.Courses;
using CloudAssignment.Application.Features.Enrollments;
using CloudAssignment.Application.Features.StudentImports;
using Microsoft.Extensions.DependencyInjection;

namespace CloudAssignment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<CourseAccessService>();
        services.AddScoped<AuditLogFactory>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IStudentImportService, StudentImportService>();
        return services;
    }
}
