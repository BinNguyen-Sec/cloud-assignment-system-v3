namespace CloudAssignment.Application.Abstractions.Importing;

public interface IStudentWorkbookService
{
    Task<ParsedStudentWorkbook> ParseAsync(
        Stream content,
        CancellationToken cancellationToken);

    byte[] CreateTemplate();

    byte[] CreateResultReport(
        IReadOnlyCollection<StudentImportReportRow> rows);
}

public sealed record ParsedStudentWorkbook(
    IReadOnlyList<ParsedStudentWorkbookRow> Rows);

public sealed record ParsedStudentWorkbookRow(
    int RowNumber,
    string? StudentCode,
    string? FullName,
    string? Email,
    bool HasFormula);

public sealed record StudentImportReportRow(
    int RowNumber,
    string? StudentCode,
    string? FullName,
    string? Email,
    string Status,
    string? Message);
