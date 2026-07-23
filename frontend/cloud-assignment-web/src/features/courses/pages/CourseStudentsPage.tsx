import { useEffect, useState, type FormEvent } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getNavigationForRole } from '../../../app/navigation';
import { EmptyState } from '../../../components/feedback/EmptyState';
import { ErrorState } from '../../../components/feedback/ErrorState';
import { LoadingSkeleton } from '../../../components/feedback/LoadingSkeleton';
import { Pagination } from '../../../components/navigation/Pagination';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../../auth/context/AuthContext';
import { courseApi } from '../api/courseApi';
import type { CourseDetail, CourseStudentPage, StudentQuery } from '../types/courseTypes';

export function CourseStudentsPage() {
  const { courseId } = useParams();
  const { user, accessToken } = useAuth();
  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [students, setStudents] = useState<CourseStudentPage | null>(null);
  const [query, setQuery] = useState<StudentQuery>({ page: 1, pageSize: 20, sort: 'fullName', direction: 'asc' });
  const [draftSearch, setDraftSearch] = useState('');
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [working, setWorking] = useState(false);

  useEffect(() => {
    if (!accessToken || !courseId) return;
    let active = true;
    setLoading(true);
    Promise.all([
      courseApi.get(accessToken, courseId),
      courseApi.listStudents(accessToken, courseId, query),
    ])
      .then(([courseResult, studentResult]) => {
        if (!active) return;
        setCourse(courseResult);
        setStudents(studentResult);
        setError(null);
      })
      .catch((caught: unknown) => {
        if (active) setError(caught instanceof ApiError ? caught.message : 'Không thể tải danh sách sinh viên.');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => { active = false; };
  }, [accessToken, courseId, query]);

  if (!user || !courseId) return null;
  const canEdit = user.role === 'Teacher';
  const basePath = `/${user.role.toLowerCase()}/courses/${courseId}`;

  async function enroll(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!accessToken || !email.trim()) return;
    setWorking(true);
    setError(null);
    try {
      await courseApi.enrollStudent(accessToken, courseId!, email.trim());
      setEmail('');
      setQuery((current) => ({ ...current, page: 1 }));
    } catch (caught: unknown) {
      setError(caught instanceof ApiError ? caught.message : 'Không thể thêm sinh viên.');
    } finally {
      setWorking(false);
    }
  }

  async function removeStudent(studentId: string, fullName: string) {
    if (!accessToken || !window.confirm(`Loại ${fullName} khỏi môn học?`)) return;
    setWorking(true);
    try {
      await courseApi.removeStudent(accessToken, courseId!, studentId);
      setQuery((current) => ({ ...current }));
    } catch (caught: unknown) {
      setError(caught instanceof ApiError ? caught.message : 'Không thể loại sinh viên.');
    } finally {
      setWorking(false);
    }
  }

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page">
        <header className="page-heading">
          <div>
            <Link className="text-link" to={basePath}>← {course?.code ?? 'Môn học'}</Link>
            <p className="academy-kicker">Enrollment Chamber</p>
            <h2>Danh sách sinh viên</h2>
            <p>{course?.name ?? 'Đang tải thông tin môn học…'}</p>
          </div>
          {canEdit ? <Link className="magic-button" to={`/teacher/courses/${courseId}/students/import`}>Import Excel</Link> : null}
        </header>

        {canEdit ? (
          <section className="manual-enrollment-panel">
            <div><h3>Thêm một sinh viên</h3><p>Email phải thuộc tài khoản Student đang hoạt động.</p></div>
            <form onSubmit={enroll}>
              <label htmlFor="student-email">Email sinh viên</label>
              <div><input id="student-email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} placeholder="student@arcana.local" /><button disabled={working}>Thêm vào lớp</button></div>
            </form>
          </section>
        ) : null}

        <section className="course-command-bar student-command-bar">
          <form onSubmit={(event) => { event.preventDefault(); setQuery((current) => ({ ...current, q: draftSearch.trim(), page: 1 })); }} className="magic-search">
            <label htmlFor="student-search">Tìm sinh viên</label>
            <div><input id="student-search" value={draftSearch} onChange={(event) => setDraftSearch(event.target.value)} placeholder="Mã SV, họ tên hoặc email…" /><button>Tìm</button></div>
          </form>
          <label>Sắp xếp<select value={query.sort} onChange={(event) => setQuery((current) => ({ ...current, sort: event.target.value as StudentQuery['sort'], page: 1 }))}><option value="fullName">Họ tên</option><option value="studentCode">Mã sinh viên</option><option value="email">Email</option><option value="enrolledAt">Ngày thêm</option></select></label>
          <label>Thứ tự<select value={query.direction} onChange={(event) => setQuery((current) => ({ ...current, direction: event.target.value as 'asc' | 'desc', page: 1 }))}><option value="asc">Tăng dần</option><option value="desc">Giảm dần</option></select></label>
        </section>

        {loading ? <LoadingSkeleton lines={7} /> : null}
        {error ? <ErrorState description={error} /> : null}
        {!loading && !error && students?.items.length === 0 ? <EmptyState title="Chưa có sinh viên" description="Thêm thủ công hoặc upload file Excel để bắt đầu." /> : null}

        {students && students.items.length > 0 ? (
          <>
            <div className="data-table-wrap">
              <table className="academy-table">
                <thead><tr><th>Mã SV</th><th>Họ tên</th><th>Email</th><th>Nguồn</th><th>Ngày thêm</th>{canEdit ? <th>Thao tác</th> : null}</tr></thead>
                <tbody>
                  {students.items.map((student) => (
                    <tr key={student.userId}>
                      <td>{student.studentCode || '—'}</td><td><strong>{student.fullName}</strong></td><td>{student.email}</td><td><span className="status-sigil">{student.enrollmentSource}</span></td><td>{formatDate(student.enrolledAtUtc)}</td>
                      {canEdit ? <td><button className="table-action table-action--danger" type="button" disabled={working} onClick={() => removeStudent(student.userId, student.fullName)}>Loại khỏi lớp</button></td> : null}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <Pagination page={students.page} totalPages={students.totalPages} hasPreviousPage={students.hasPreviousPage} hasNextPage={students.hasNextPage} onPageChange={(page) => setQuery((current) => ({ ...current, page }))} />
          </>
        ) : null}
      </div>
    </AcademyShell>
  );
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
}
