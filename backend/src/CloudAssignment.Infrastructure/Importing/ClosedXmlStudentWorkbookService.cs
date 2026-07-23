using ClosedXML.Excel;
using CloudAssignment.Application.Abstractions.Importing;

namespace CloudAssignment.Infrastructure.Importing;

public sealed class ClosedXmlStudentWorkbookService : IStudentWorkbookService
{
    private const string StudentsWorksheetName = "Students";

    public Task<ParsedStudentWorkbook> ParseAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var workbook = new XLWorkbook(content);
        var worksheet = workbook.Worksheets.FirstOrDefault(sheet =>
                string.Equals(sheet.Name, StudentsWorksheetName, StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidDataException("Workbook does not contain a worksheet.");

        var headers = ReadHeaders(worksheet);
        if (!headers.TryGetValue("email", out var emailColumn))
        {
            throw new InvalidDataException("Worksheet must contain an Email column.");
        }

        headers.TryGetValue("studentcode", out var studentCodeColumn);
        headers.TryGetValue("fullname", out var fullNameColumn);
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        var rows = new List<ParsedStudentWorkbookRow>();

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var emailCell = worksheet.Cell(rowNumber, emailColumn);
            var studentCodeCell = studentCodeColumn == 0
                ? null
                : worksheet.Cell(rowNumber, studentCodeColumn);
            var fullNameCell = fullNameColumn == 0
                ? null
                : worksheet.Cell(rowNumber, fullNameColumn);
            var studentCode = ReadCell(studentCodeCell);
            var fullName = ReadCell(fullNameCell);
            var email = ReadCell(emailCell);

            if (studentCode is null && fullName is null && email is null)
            {
                continue;
            }

            rows.Add(new ParsedStudentWorkbookRow(
                rowNumber,
                studentCode,
                fullName,
                email,
                emailCell.HasFormula ||
                (studentCodeCell?.HasFormula ?? false) ||
                (fullNameCell?.HasFormula ?? false)));
        }

        return Task.FromResult(new ParsedStudentWorkbook(rows));
    }

    public byte[] CreateTemplate()
    {
        using var workbook = new XLWorkbook();
        var students = workbook.Worksheets.Add(StudentsWorksheetName);
        students.Cell(1, 1).Value = "StudentCode";
        students.Cell(1, 2).Value = "FullName";
        students.Cell(1, 3).Value = "Email";
        StyleHeader(students.Range(1, 1, 1, 3));
        students.Column(1).Width = 20;
        students.Column(2).Width = 32;
        students.Column(3).Width = 38;
        students.SheetView.FreezeRows(1);
        students.Range("A1:C1001").SetAutoFilter();

        var instructions = workbook.Worksheets.Add("Instructions");
        instructions.Cell("A1").Value = "HƯỚNG DẪN IMPORT SINH VIÊN";
        instructions.Cell("A3").Value = "1. Không đổi tên sheet Students hoặc tên cột.";
        instructions.Cell("A4").Value = "2. Email là bắt buộc và phải thuộc tài khoản Student đã có trong hệ thống.";
        instructions.Cell("A5").Value = "3. StudentCode và FullName dùng để đối chiếu, có thể để trống.";
        instructions.Cell("A6").Value = "4. Không dùng công thức trong ba cột định danh.";
        instructions.Cell("A7").Value = "5. Tối đa 1.000 dòng dữ liệu và 5 MB.";
        instructions.Column(1).Width = 100;
        instructions.Cell("A1").Style.Font.Bold = true;
        instructions.Cell("A1").Style.Font.FontSize = 16;

        return Save(workbook);
    }

    public byte[] CreateResultReport(
        IReadOnlyCollection<StudentImportReportRow> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ImportResult");
        var headers = new[]
        {
            "Row",
            "StudentCode",
            "FullName",
            "Email",
            "FinalStatus",
            "Message"
        };

        for (var index = 0; index < headers.Length; index++)
        {
            worksheet.Cell(1, index + 1).Value = headers[index];
        }

        StyleHeader(worksheet.Range(1, 1, 1, headers.Length));
        var targetRow = 2;
        foreach (var row in rows)
        {
            worksheet.Cell(targetRow, 1).Value = row.RowNumber;
            worksheet.Cell(targetRow, 2).Value = row.StudentCode ?? string.Empty;
            worksheet.Cell(targetRow, 3).Value = row.FullName ?? string.Empty;
            worksheet.Cell(targetRow, 4).Value = row.Email ?? string.Empty;
            worksheet.Cell(targetRow, 5).Value = row.Status;
            worksheet.Cell(targetRow, 6).Value = row.Message ?? string.Empty;
            targetRow++;
        }

        worksheet.Columns().AdjustToContents(1, 60);
        worksheet.SheetView.FreezeRows(1);
        if (targetRow > 2)
        {
            worksheet.Range(1, 1, targetRow - 1, headers.Length).SetAutoFilter();
        }

        return Save(workbook);
    }

    private static Dictionary<string, int> ReadHeaders(IXLWorksheet worksheet)
    {
        var lastColumn = worksheet.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var column = 1; column <= lastColumn; column++)
        {
            var header = worksheet.Cell(1, column).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(header))
            {
                headers[NormalizeHeader(header)] = column;
            }
        }

        return headers;
    }

    private static string NormalizeHeader(string value) =>
        value.Replace(" ", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();

    private static string? ReadCell(IXLCell? cell)
    {
        if (cell is null)
        {
            return null;
        }

        var value = cell.GetString().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#5964D8");
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
