using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CloudAssignment.Application.Abstractions.Authentication;

namespace CloudAssignment.Api.Authentication;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
