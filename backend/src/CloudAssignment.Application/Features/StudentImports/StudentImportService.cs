using System.ComponentModel.DataAnnotations;
using CloudAssignment.Application.Abstractions.Importing;
using CloudAssignment.Application.Abstractions.Persistence;
using CloudAssignment.Application.Abstractions.Time;
using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Application.Common.Models;
using CloudAssignment.Application.Features.Courses;
using CloudAssignment.Domain.Courses;
using CloudAssignment.Domain.StudentImports;
using CloudAssignment.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CloudAssignment.Application.Features.StudentImports;

internal sealed class StudentImportService(
    IApplicationDbContext dbContext,
    IStudentWorkbookService workbookService,
    CourseAccessService accessService,
    AuditLogFactory auditLogFactory,
    IClock clock) : IStudentImportService
{
    private const long MaximumFileSizeBytes = 5 * 1024 * 1024;
    private const int MaximumRows = 1000;
    private static readonly TimeSpan PreviewLifetime = TimeSpan.FromMinutes(30);
    private const string WorkbookContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public WorkbookFileDto CreateTemplate() =>
        new(
            workbookService.CreateTemplate(),
            "student-import-template.xlsx",
            WorkbookContentType);

    public async Task<StudentImportPreviewDto> PreviewAsync(
        Guid courseId,
        string fileName,
        long fileSize,
        Stream content,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, user) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        ValidateFile(fileName, fileSize);
        ParsedStudentWorkbook workbook;
        try
        {
            workbook = await workbookService.ParseAsync(content, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            throw Validation("file", $"Không thể đọc file Excel: {exception.Message}");
        }

        if (workbook.Rows.Count == 0)
        {
            throw Validation("file", "File Excel không có dòng sinh viên nào.");
        }

        if (workbook.Rows.Count > MaximumRows)
        {
            throw Validation("file", $"File Excel chỉ được chứa tối đa {MaximumRows} dòng dữ liệu.");
        }

        var normalizedEmails = workbook.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Email))
            .Select(row => NormalizeEmail(row.Email!))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(candidate => normalizedEmails.Contains(candidate.NormalizedEmail))
            .ToDictionaryAsync(candidate => candidate.NormalizedEmail, StringComparer.Ordinal, cancellationToken);

        var candidateStudentIds = users.Values
            .Where(candidate => candidate.Role == UserRole.Student)
            .Select(candidate => candidate.Id)
            .ToList();
        var enrolledStudentIds = await dbContext.CourseMembers
            .AsNoTracking()
            .Where(member => member.CourseId == courseId && candidateStudentIds.Contains(member.StudentId))
            .Select(member => member.StudentId)
            .ToHashSetAsync(cancellationToken);

        var duplicateEmails = workbook.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Email))
            .GroupBy(row => NormalizeEmail(row.Email!), StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        var batchId = Guid.NewGuid();
        var classifiedRows = workbook.Rows
            .Select(row => ClassifyRow(row, users, enrolledStudentIds, duplicateEmails))
            .ToList();
        var validRows = classifiedRows.Count(row => row.Status == StudentImportRowStatus.Valid);
        var utcNow = clock.UtcNow;
        var batch = StudentImportBatch.CreatePreview(
            batchId,
            courseId,
            user.Id,
            Path.GetFileName(fileName),
            classifiedRows.Count,
            validRows,
            classifiedRows.Count - validRows,
            utcNow,
            utcNow.Add(PreviewLifetime));

        dbContext.StudentImportBatches.Add(batch);
        foreach (var row in classifiedRows)
        {
            dbContext.StudentImportRows.Add(StudentImportRow.Create(
                Guid.NewGuid(),
                batchId,
                row.RowNumber,
                row.StudentCode,
                row.FullName,
                row.Email,
                row.ResolvedUserId,
                row.Status,
                row.Message));
        }

        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "STUDENT_IMPORT_PREVIEWED",
            nameof(StudentImportBatch),
            batch.Id,
            new
            {
                course.Id,
                course.Code,
                FileName = batch.OriginalFileName,
                batch.TotalRows,
                batch.ValidRows,
                batch.InvalidRows
            },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapPreview(batch, classifiedRows.Select(MapRow).ToList());
    }

    public async Task<StudentImportConfirmDto> ConfirmAsync(
        Guid courseId,
        Guid batchId,
        AuditRequestContext auditContext,
        CancellationToken cancellationToken)
    {
        var (course, _) = await accessService.RequireOwnedCourseAsync(courseId, cancellationToken);
        var batch = await dbContext.StudentImportBatches.SingleOrDefaultAsync(
            candidate => candidate.Id == batchId && candidate.CourseId == courseId,
            cancellationToken)
            ?? throw ImportNotFound();
        var rows = await dbContext.StudentImportRows
            .Where(row => row.BatchId == batchId)
            .OrderBy(row => row.RowNumber)
            .ToListAsync(cancellationToken);

        if (batch.Status == StudentImportBatchStatus.Completed)
        {
            return MapConfirm(batch, rows);
        }

        if (batch.Status != StudentImportBatchStatus.Previewed)
        {
            throw new ConflictException("IMPORT_NOT_CONFIRMABLE", "Batch import không còn ở trạng thái có thể xác nhận.");
        }

        var utcNow = clock.UtcNow;
        if (batch.IsExpiredAt(utcNow))
        {
            batch.MarkExpired();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new ConflictException("IMPORT_PREVIEW_EXPIRED", "Bản xem trước đã hết hạn. Vui lòng upload lại file.");
        }

        var validRows = rows
            .Where(row => row.Status == StudentImportRowStatus.Valid && row.ResolvedUserId is not null)
            .ToList();
        var studentIds = validRows.Select(row => row.ResolvedUserId!.Value).Distinct().ToList();
        var users = await dbContext.Users
            .Where(user => studentIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);
        var enrolledStudentIds = await dbContext.CourseMembers
            .Where(member => member.CourseId == courseId && studentIds.Contains(member.StudentId))
            .Select(member => member.StudentId)
            .ToHashSetAsync(cancellationToken);

        var importedRows = 0;
        foreach (var row in validRows)
        {
            var studentId = row.ResolvedUserId!.Value;
            if (!users.TryGetValue(studentId, out var student))
            {
                row.MarkSkipped(StudentImportRowStatus.UserNotFound, "Tài khoản không còn tồn tại.");
                continue;
            }

            if (student.Role != UserRole.Student)
            {
                row.MarkSkipped(StudentImportRowStatus.WrongRole, "Tài khoản không còn vai trò Student.");
                continue;
            }

            if (!student.IsActive)
            {
                row.MarkSkipped(StudentImportRowStatus.InactiveUser, "Tài khoản đã bị vô hiệu hóa.");
                continue;
            }

            if (enrolledStudentIds.Contains(studentId))
            {
                row.MarkSkipped(StudentImportRowStatus.AlreadyEnrolled, "Sinh viên đã có trong môn học.");
                continue;
            }

            dbContext.CourseMembers.Add(CourseMember.Create(
                Guid.NewGuid(),
                courseId,
                studentId,
                EnrollmentSource.Excel,
                batch.Id,
                utcNow));
            enrolledStudentIds.Add(studentId);
            row.MarkImported();
            importedRows++;
        }

        var skippedRows = rows.Count - importedRows;
        batch.Complete(importedRows, skippedRows, utcNow);
        dbContext.AuditLogs.Add(auditLogFactory.Create(
            "STUDENT_IMPORT_CONFIRMED",
            nameof(StudentImportBatch),
            batch.Id,
            new
            {
                course.Id,
                course.Code,
                batch.TotalRows,
                ImportedRows = importedRows,
                SkippedRows = skippedRows
            },
            auditContext));
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapConfirm(batch, rows);
    }

    public async Task<PagedResponse<StudentImportHistoryDto>> GetHistoryAsync(
        StudentImportHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var user = await accessService.RequireCurrentUserAsync(cancellationToken);
        if (user.Role is not (UserRole.Teacher or UserRole.Admin))
        {
            throw new ForbiddenException("IMPORT_HISTORY_FORBIDDEN", "Bạn không có quyền xem lịch sử import.");
        }

        var page = CreatePageRequest(request.Page, request.PageSize);
        var query = dbContext.StudentImportBatches.AsNoTracking();
        if (user.Role == UserRole.Teacher)
        {
            query = query.Where(batch => dbContext.Courses.Any(
                course => course.Id == batch.CourseId && course.TeacherId == user.Id));
        }

        if (request.CourseId is not null)
        {
            query = query.Where(batch => batch.CourseId == request.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<StudentImportBatchStatus>(request.Status, ignoreCase: true, out var status))
            {
                throw Validation("status", "Trạng thái import không hợp lệ.");
            }

            query = query.Where(batch => batch.Status == status);
        }

        var totalItems = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(batch => batch.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(batch => new StudentImportHistoryRow(
                batch.Id,
                batch.CourseId,
                dbContext.Courses
                    .Where(course => course.Id == batch.CourseId)
                    .Select(course => course.Code)
                    .Single(),
                dbContext.Courses
                    .Where(course => course.Id == batch.CourseId)
                    .Select(course => course.Name)
                    .Single(),
                batch.OriginalFileName,
                batch.Status,
                batch.TotalRows,
                batch.ValidRows,
                batch.InvalidRows,
                batch.ImportedRows,
                batch.SkippedRows,
                batch.CreatedAtUtc,
                batch.CompletedAtUtc,
                batch.ExpiresAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResponse<StudentImportHistoryDto>(
            rows.Select(MapHistory).ToList(),
            page.Page,
            page.PageSize,
            totalItems);
    }

    public async Task<StudentImportPreviewDto> GetBatchAsync(
        Guid courseId,
        Guid batchId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireCourseAccessAsync(courseId, cancellationToken);
        var batch = await dbContext.StudentImportBatches
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == batchId && candidate.CourseId == courseId,
                cancellationToken)
            ?? throw ImportNotFound();
        var rows = await dbContext.StudentImportRows
            .AsNoTracking()
            .Where(row => row.BatchId == batchId)
            .OrderBy(row => row.RowNumber)
            .ToListAsync(cancellationToken);

        return MapPreview(batch, rows.Select(MapPersistedRow).ToList());
    }

    public async Task<WorkbookFileDto> CreateErrorReportAsync(
        Guid courseId,
        Guid batchId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireCourseAccessAsync(courseId, cancellationToken);
        var batch = await dbContext.StudentImportBatches
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.Id == batchId && candidate.CourseId == courseId,
                cancellationToken)
            ?? throw ImportNotFound();
        var rows = await dbContext.StudentImportRows
            .AsNoTracking()
            .Where(row => row.BatchId == batchId && row.Status != StudentImportRowStatus.Imported)
            .OrderBy(row => row.RowNumber)
            .ToListAsync(cancellationToken);
        var reportRows = rows.Select(row => new StudentImportReportRow(
            row.RowNumber,
            row.StudentCode,
            row.FullName,
            row.Email,
            row.Status.ToString(),
            row.Message)).ToList();
        var content = workbookService.CreateResultReport(reportRows);
        return new WorkbookFileDto(
            content,
            $"student-import-result-{batch.Id:N}.xlsx",
            WorkbookContentType);
    }

    private static ClassifiedRow ClassifyRow(
        ParsedStudentWorkbookRow row,
        Dictionary<string, User> users,
        HashSet<Guid> enrolledStudentIds,
        HashSet<string> duplicateEmails)
    {
        if (row.HasFormula)
        {
            return Invalid(row, StudentImportRowStatus.Invalid, "Không chấp nhận công thức trong các cột định danh.");
        }

        if (string.IsNullOrWhiteSpace(row.Email) ||
            !new EmailAddressAttribute().IsValid(row.Email.Trim()))
        {
            return Invalid(row, StudentImportRowStatus.Invalid, "Email là bắt buộc và phải đúng định dạng.");
        }

        var normalizedEmail = NormalizeEmail(row.Email);
        if (duplicateEmails.Contains(normalizedEmail))
        {
            return Invalid(row, StudentImportRowStatus.DuplicateInFile, "Email bị trùng trong cùng file.");
        }

        if (!users.TryGetValue(normalizedEmail, out var user))
        {
            return Invalid(row, StudentImportRowStatus.UserNotFound, "Không tìm thấy tài khoản trong hệ thống.");
        }

        if (user.Role != UserRole.Student)
        {
            return Invalid(row, StudentImportRowStatus.WrongRole, "Tài khoản không có vai trò Student.", user.Id);
        }

        if (!user.IsActive)
        {
            return Invalid(row, StudentImportRowStatus.InactiveUser, "Tài khoản đang bị vô hiệu hóa.", user.Id);
        }

        if (enrolledStudentIds.Contains(user.Id))
        {
            return Invalid(row, StudentImportRowStatus.AlreadyEnrolled, "Sinh viên đã có trong môn học.", user.Id);
        }

        return new ClassifiedRow(
            row.RowNumber,
            row.StudentCode,
            row.FullName,
            row.Email?.Trim(),
            user.Id,
            StudentImportRowStatus.Valid,
            "Sẵn sàng thêm vào môn học.");
    }

    private static ClassifiedRow Invalid(
        ParsedStudentWorkbookRow row,
        StudentImportRowStatus status,
        string message,
        Guid? resolvedUserId = null) =>
        new(
            row.RowNumber,
            row.StudentCode,
            row.FullName,
            row.Email?.Trim(),
            resolvedUserId,
            status,
            message);

    private static StudentImportPreviewDto MapPreview(
        StudentImportBatch batch,
        IReadOnlyList<StudentImportRowDto> rows) =>
        new(
            batch.Id,
            batch.OriginalFileName,
            batch.TotalRows,
            batch.ValidRows,
            batch.InvalidRows,
            batch.ExpiresAtUtc,
            rows);

    private static StudentImportConfirmDto MapConfirm(
        StudentImportBatch batch,
        IReadOnlyCollection<StudentImportRow> rows) =>
        new(
            batch.Id,
            batch.Status.ToString(),
            batch.ImportedRows,
            batch.SkippedRows,
            batch.CompletedAtUtc,
            rows.OrderBy(row => row.RowNumber).Select(row => new StudentImportRowDto(
                row.RowNumber,
                row.StudentCode,
                row.FullName,
                row.Email,
                row.Status.ToString(),
                row.Message)).ToList());

    private static StudentImportRowDto MapRow(ClassifiedRow row) =>
        new(
            row.RowNumber,
            row.StudentCode,
            row.FullName,
            row.Email,
            row.Status.ToString(),
            row.Message);

    private static StudentImportHistoryDto MapHistory(StudentImportHistoryRow row) =>
        new(
            row.BatchId,
            row.CourseId,
            row.CourseCode,
            row.CourseName,
            row.OriginalFileName,
            row.Status.ToString(),
            row.TotalRows,
            row.ValidRows,
            row.InvalidRows,
            row.ImportedRows,
            row.SkippedRows,
            row.CreatedAtUtc,
            row.CompletedAtUtc,
            row.ExpiresAtUtc);

    private static StudentImportRowDto MapPersistedRow(StudentImportRow row) =>
        new(
            row.RowNumber,
            row.StudentCode,
            row.FullName,
            row.Email,
            row.Status.ToString(),
            row.Message);

    private sealed record StudentImportHistoryRow(
        Guid BatchId,
        Guid CourseId,
        string CourseCode,
        string CourseName,
        string OriginalFileName,
        StudentImportBatchStatus Status,
        int TotalRows,
        int ValidRows,
        int InvalidRows,
        int ImportedRows,
        int SkippedRows,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? CompletedAtUtc,
        DateTimeOffset ExpiresAtUtc);

    private static void ValidateFile(string fileName, long fileSize)
    {
        var errors = new Dictionary<string, string[]>();
        if (!string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            errors["file"] = ["Chỉ chấp nhận file Excel định dạng .xlsx."];
        }
        else if (fileSize is <= 0 or > MaximumFileSizeBytes)
        {
            errors["file"] = ["File Excel phải có dung lượng từ 1 byte đến 5 MB."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }

    private static PageRequest CreatePageRequest(int page, int pageSize)
    {
        try
        {
            return new PageRequest(page, pageSize);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw Validation("pagination", "Page phải từ 1 và pageSize phải nằm trong khoảng 1–50.");
        }
    }

    private static RequestValidationException Validation(string field, string message) =>
        new(new Dictionary<string, string[]> { [field] = [message] });

    private static NotFoundException ImportNotFound() =>
        new("IMPORT_NOT_FOUND", "Batch import không tồn tại hoặc nằm ngoài phạm vi truy cập.");

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    private sealed record ClassifiedRow(
        int RowNumber,
        string? StudentCode,
        string? FullName,
        string? Email,
        Guid? ResolvedUserId,
        StudentImportRowStatus Status,
        string? Message);
}
