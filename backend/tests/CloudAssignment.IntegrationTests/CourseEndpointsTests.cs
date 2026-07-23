using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudAssignment.Application.Common.Models;
using CloudAssignment.Application.Features.Auth;
using CloudAssignment.Application.Features.Courses;
using CloudAssignment.Application.Features.Enrollments;
using CloudAssignment.Application.Features.StudentImports;

namespace CloudAssignment.IntegrationTests;

public sealed class CourseEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task TeacherCanCreateSearchAndReadOwnedCourse()
    {
        await AuthenticateTeacherAsync();
        var code = $"CLOUD-{Guid.NewGuid():N}"[..18].ToUpperInvariant();
        var createResponse = await _client.PostAsJsonAsync(
            "/api/v1/courses",
            new CreateCourseRequest(
                code,
                "Cloud Security",
                "Course integration test",
                "Semester 1",
                "2026-2027",
                "astral"));
        var created = await createResponse.Content.ReadFromJsonAsync<CourseDetailDto>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(code, created.Code);

        var list = await _client.GetFromJsonAsync<PagedResponse<CourseSummaryDto>>(
            $"/api/v1/courses?q={Uri.EscapeDataString(code)}&page=1&pageSize=20&archived=false");
        Assert.NotNull(list);
        Assert.Contains(list.Items, course => course.Id == created.Id);

        var detail = await _client.GetFromJsonAsync<CourseDetailDto>($"/api/v1/courses/{created.Id}");
        Assert.NotNull(detail);
        Assert.True(detail.CanManage);
    }

    [Fact]
    public async Task TeacherCanEnrollStudentManually()
    {
        await AuthenticateTeacherAsync();
        var course = await CreateCourseAsync("Manual Enrollment");
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/courses/{course.Id}/students",
            new EnrollStudentRequest("student2@arcana.local"));
        var enrolled = await response.Content.ReadFromJsonAsync<CourseStudentDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(enrolled);
        Assert.Equal("student2@arcana.local", enrolled.Email);
        Assert.Equal("Manual", enrolled.EnrollmentSource);

        var students = await _client.GetFromJsonAsync<PagedResponse<CourseStudentDto>>(
            $"/api/v1/courses/{course.Id}/students?page=1&pageSize=20");
        Assert.NotNull(students);
        Assert.Contains(students.Items, student => student.Email == "student2@arcana.local");
    }

    [Fact]
    public async Task ExcelPreviewAndConfirmImportOnlyValidStudentRows()
    {
        await AuthenticateTeacherAsync();
        var course = await CreateCourseAsync("Excel Enrollment");
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "student-import-valid.xlsx");
        await using var stream = File.OpenRead(fixturePath);
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        form.Add(fileContent, "file", "student-import-valid.xlsx");

        var previewResponse = await _client.PostAsync(
            $"/api/v1/courses/{course.Id}/students/import-preview",
            form);
        var preview = await previewResponse.Content.ReadFromJsonAsync<StudentImportPreviewDto>();

        Assert.Equal(HttpStatusCode.OK, previewResponse.StatusCode);
        Assert.NotNull(preview);
        Assert.Equal(2, preview.TotalRows);
        Assert.Equal(1, preview.ValidRows);
        Assert.Equal(1, preview.InvalidRows);

        var confirmResponse = await _client.PostAsync(
            $"/api/v1/courses/{course.Id}/students/imports/{preview.BatchId}/confirm",
            content: null);
        var result = await confirmResponse.Content.ReadFromJsonAsync<StudentImportConfirmDto>();

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.ImportedRows);
        Assert.Contains(result.Rows, row => row.Email == "student3@arcana.local" && row.Status == "Imported");
    }

    [Fact]
    public async Task StudentCannotCreateCourse()
    {
        await AuthenticateAsync("student@arcana.local");
        var response = await _client.PostAsJsonAsync(
            "/api/v1/courses",
            new CreateCourseRequest(
                "DENIED-101",
                "Denied Course",
                null,
                null,
                null,
                "runes"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<CourseDetailDto> CreateCourseAsync(string name)
    {
        var code = $"T-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var response = await _client.PostAsJsonAsync(
            "/api/v1/courses",
            new CreateCourseRequest(code, name, null, "Semester 1", "2026-2027", "runes"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseDetailDto>()
            ?? throw new InvalidOperationException("Course response was empty.");
    }

    private Task AuthenticateTeacherAsync() => AuthenticateAsync("teacher@arcana.local");

    private async Task AuthenticateAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, "Arcana@Test2026!"));
        var session = await response.Content.ReadFromJsonAsync<AuthSessionDto>();
        Assert.NotNull(session);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AccessToken);
    }
}
