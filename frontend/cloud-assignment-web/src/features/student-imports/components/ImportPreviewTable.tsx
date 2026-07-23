import type { StudentImportRow } from '../types/studentImportTypes';

export function ImportPreviewTable({ rows }: { rows: StudentImportRow[] }) {
  return (
    <div className="data-table-wrap import-preview-table">
      <table className="academy-table">
        <thead><tr><th>Dòng</th><th>Mã SV</th><th>Họ tên</th><th>Email</th><th>Trạng thái</th><th>Chi tiết</th></tr></thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.rowNumber} className={`import-row import-row--${row.status.toLowerCase()}`}>
              <td>{row.rowNumber}</td><td>{row.studentCode || '—'}</td><td>{row.fullName || '—'}</td><td>{row.email || '—'}</td><td><span className="status-sigil">{statusLabel(row.status)}</span></td><td>{row.message || '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function statusLabel(status: StudentImportRow['status']) {
  const labels: Record<StudentImportRow['status'], string> = {
    Valid: 'Hợp lệ', Invalid: 'Không hợp lệ', DuplicateInFile: 'Trùng trong file', AlreadyEnrolled: 'Đã trong lớp', UserNotFound: 'Không tìm thấy', InactiveUser: 'Đã khóa', WrongRole: 'Sai vai trò', Imported: 'Đã import',
  };
  return labels[status];
}
