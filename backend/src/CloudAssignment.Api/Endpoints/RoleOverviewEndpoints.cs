using CloudAssignment.Api.Authentication;
using CloudAssignment.Application.Features.Auth;

namespace CloudAssignment.Api.Endpoints;

public static class RoleOverviewEndpoints
{
    public static IEndpointRouteBuilder MapRoleOverviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/overview", GetOverviewAsync)
            .WithTags("Admin")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        endpoints.MapGet("/api/v1/teacher/overview", GetOverviewAsync)
            .WithTags("Teacher")
            .RequireAuthorization(AuthorizationPolicies.TeacherOnly);

        endpoints.MapGet("/api/v1/student/overview", GetOverviewAsync)
            .WithTags("Student")
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);

        return endpoints;
    }

    private static async Task<IResult> GetOverviewAsync(
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var user = await authenticationService.GetCurrentUserAsync(cancellationToken);
        return Results.Ok(new
        {
            user,
            phase = "Course Management",
            message = "Course Library, enrollment và Excel import đã sẵn sàng theo phạm vi vai trò."
        });
    }
}
