import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getNavigationForRole } from '../../../app/navigation';
import { EmptyState } from '../../../components/feedback/EmptyState';
import { ErrorState } from '../../../components/feedback/ErrorState';
import { LoadingSkeleton } from '../../../components/feedback/LoadingSkeleton';
import { Pagination } from '../../../components/navigation/Pagination';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../../auth/context/AuthContext';
import { courseApi } from '../../courses/api/courseApi';
import type { StudentImportHistoryPage } from '../types/studentImportTypes';

export function ImportHistoryPage() {
  const { user, accessToken } = useAuth();
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState('');
  const [data, setData] = useState<StudentImportHistoryPage | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!accessToken) return;
    setLoading(true);
    courseApi.listImports(accessToken, { page, pageSize: 20, status })
      .then((result) => { setData(result); setError(null); })
      .catch((caught: unknown) => setError(caught instanceof ApiError ? caught.message : 'Không thể tải lịch sử import.'))
      .finally(() => setLoading(false));
  }, [accessToken, page, status]);

  if (!user || !['Teacher', 'Admin'].includes(user.role)) return null;

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page">
        <header className="page-heading"><div><p className="academy-kicker">Import Chronicle</p><h2>Lịch sử import sinh viên</h2><p>Theo dõi batch, số dòng hợp lệ, số sinh viên đã thêm và báo cáo lỗi.</p></div><label>Trạng thái<select value={status} onChange={(event) => { setStatus(event.target.value); setPage(1); }}><option value="">Tất cả</option><option value="Previewed">Đang preview</option><option value="Completed">Hoàn tất</option><option value="Expired">Hết hạn</option></select></label></header>
        {loading ? <LoadingSkeleton lines={6} /> : null}
        {error ? <ErrorState description={error} /> : null}
        {!loading && !error && data?.items.length === 0 ? <EmptyState title="Chưa có lịch sử import" description="Các batch Excel sẽ xuất hiện tại đây." /> : null}
        {data && data.items.length > 0 ? <><div className="data-table-wrap"><table className="academy-table"><thead><tr><th>Môn học</th><th>File</th><th>Trạng thái</th><th>Tổng</th><th>Hợp lệ</th><th>Đã thêm</th><th>Thời gian</th><th></th></tr></thead><tbody>{data.items.map((item) => <tr key={item.batchId}><td><strong>{item.courseCode}</strong><br /><small>{item.courseName}</small></td><td>{item.originalFileName}</td><td><span className="status-sigil">{item.status}</span></td><td>{item.totalRows}</td><td>{item.validRows}</td><td>{item.importedRows}</td><td>{formatDate(item.createdAtUtc)}</td><td><Link className="table-action" to={`/${user.role.toLowerCase()}/courses/${item.courseId}/students`}>Mở lớp</Link></td></tr>)}</tbody></table></div><Pagination page={data.page} totalPages={data.totalPages} hasPreviousPage={data.hasPreviousPage} hasNextPage={data.hasNextPage} onPageChange={setPage} /></> : null}
      </div>
    </AcademyShell>
  );
}

function formatDate(value: string) { return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)); }
