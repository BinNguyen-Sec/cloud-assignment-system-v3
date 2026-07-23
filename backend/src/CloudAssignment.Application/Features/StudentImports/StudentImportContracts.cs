using CloudAssignment.Application.Common.Models;
using CloudAssignment.Application.Features.Courses;

namespace CloudAssignment.Application.Features.StudentImports;

public sealed record StudentImportPreviewDto(
    Guid BatchId,
    string FileName,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<StudentImportRowDto> Rows);

public sealed record StudentImportRowDto(
    int RowNumber,
    string? StudentCode,
    string? FullName,
    string? Email,
    string Status,
    string? Message);

public sealed record StudentImportConfirmDto(
    Guid BatchId,
    string Status,
    int ImportedRows,
    int SkippedRows,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<StudentImportRowDto> Rows);

public sealed record StudentImportHistoryDto(
    Guid BatchId,
    Guid CourseId,
    string CourseCode,
    string CourseName,
    string OriginalFileName,
    string Status,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    int ImportedRows,
    int SkippedRows,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset ExpiresAtUtc);

public sealed record StudentImportHistoryRequest(
    int Page = PageRequest.DefaultPage,
    int PageSize = PageRequest.DefaultPageSize,
    string? Status = null,
    Guid? CourseId = null);

public sealed record WorkbookFileDto(
    byte[] Content,
    string FileName,
    string ContentType);

public interface IStudentImportService
{
    WorkbookFileDto CreateTemplate();

    Task<StudentImportPreviewDto> PreviewAsync(
        Guid courseId,
        string fileName,
        long fileSize,
        Stream content,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task<StudentImportConfirmDto> ConfirmAsync(
        Guid courseId,
        Guid batchId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken);

    Task<PagedResponse<StudentImportHistoryDto>> GetHistoryAsync(
        StudentImportHistoryRequest request,
        CancellationToken cancellationToken);

    Task<StudentImportPreviewDto> GetBatchAsync(
        Guid courseId,
        Guid batchId,
        CancellationToken cancellationToken);

    Task<WorkbookFileDto> CreateErrorReportAsync(
        Guid courseId,
        Guid batchId,
        CancellationToken cancellationToken);
}
