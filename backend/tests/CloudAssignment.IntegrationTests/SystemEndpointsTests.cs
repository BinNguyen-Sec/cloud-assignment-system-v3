using System.Net;
using System.Net.Http.Json;

namespace CloudAssignment.IntegrationTests;

public sealed class SystemEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LiveHealthReturnsOk()
    {
        var response = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SystemInfoReturnsApplicationIdentity()
    {
        var response = await _client.GetAsync("/api/v1/system/info");
        var payload = await response.Content.ReadFromJsonAsync<SystemInfoResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Cloud Assignment System V3", payload.Name);
    }

    private sealed record SystemInfoResponse(string Name, string Version, string Environment);
}
