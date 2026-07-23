using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudAssignment.Application.Features.Auth;

namespace CloudAssignment.IntegrationTests;

public sealed class AuthenticationEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LoginWithTeacherAccountReturnsSession()
    {
        var response = await LoginAsync("teacher@arcana.local");
        var session = await response.Content.ReadFromJsonAsync<AuthSessionDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(session);
        Assert.Equal("Teacher", session.User.Role);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }

    [Fact]
    public async Task InvalidPasswordReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("teacher@arcana.local", "WrongPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TeacherTokenCanOpenTeacherOverviewButNotAdminOverview()
    {
        var loginResponse = await LoginAsync("teacher@arcana.local");
        var session = await loginResponse.Content.ReadFromJsonAsync<AuthSessionDto>();
        Assert.NotNull(session);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var teacherResponse = await _client.GetAsync("/api/v1/teacher/overview");
        var adminResponse = await _client.GetAsync("/api/v1/admin/overview");

        Assert.Equal(HttpStatusCode.OK, teacherResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);
    }

    [Fact]
    public async Task RefreshCookieRotatesSession()
    {
        var loginResponse = await LoginAsync("student@arcana.local");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var refreshResponse = await _client.PostAsync("/api/v1/auth/refresh", content: null);
        var session = await refreshResponse.Content.ReadFromJsonAsync<AuthSessionDto>();

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        Assert.NotNull(session);
        Assert.Equal("Student", session.User.Role);
    }

    private Task<HttpResponseMessage> LoginAsync(string email) =>
        _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, "Arcana@Test2026!"));
}
