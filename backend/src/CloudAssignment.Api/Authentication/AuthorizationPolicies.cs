using CloudAssignment.Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace CloudAssignment.Api.Authentication;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string TeacherOnly = "TeacherOnly";
    public const string StudentOnly = "StudentOnly";

    public static void AddRolePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(AdminOnly, policy => policy.RequireRole(UserRole.Admin.ToString()));
        options.AddPolicy(TeacherOnly, policy => policy.RequireRole(UserRole.Teacher.ToString()));
        options.AddPolicy(StudentOnly, policy => policy.RequireRole(UserRole.Student.ToString()));
    }
}
