import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getNavigationForRole } from '../../../app/navigation';
import { ErrorState } from '../../../components/feedback/ErrorState';
import { LoadingSkeleton } from '../../../components/feedback/LoadingSkeleton';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../../auth/context/AuthContext';
import { courseApi } from '../api/courseApi';
import type { CourseDetail } from '../types/courseTypes';

export function CourseDetailPage() {
  const { courseId } = useParams();
  const { user, accessToken } = useAuth();
  const navigate = useNavigate();
  const [course, setCourse] = useState<CourseDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [working, setWorking] = useState(false);

  useEffect(() => {
    if (!courseId || !accessToken) return;
    courseApi.get(accessToken, courseId)
      .then(setCourse)
      .catch((caught: unknown) => setError(caught instanceof ApiError ? caught.message : 'Không thể tải môn học.'));
  }, [accessToken, courseId]);

  if (!user || !courseId) return null;
  const basePath = `/${user.role.toLowerCase()}/courses`;

  async function toggleArchive() {
    if (!accessToken || !course) return;
    setWorking(true);
    try {
      if (course.isArchived) await courseApi.restore(accessToken, course.id);
      else await courseApi.archive(accessToken, course.id);
      setCourse(await courseApi.get(accessToken, course.id));
    } catch (caught: unknown) {
      setError(caught instanceof ApiError ? caught.message : 'Không thể cập nhật trạng thái môn học.');
    } finally {
      setWorking(false);
    }
  }

  async function deleteCourse() {
    if (!accessToken || !course) return;
    if (!window.confirm(`Xóa vĩnh viễn môn ${course.code}?`)) return;
    setWorking(true);
    try {
      await courseApi.remove(accessToken, course.id);
      navigate(basePath, { replace: true });
    } catch (caught: unknown) {
      setError(caught instanceof ApiError ? caught.message : 'Không thể xóa môn học.');
      setWorking(false);
    }
  }

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page">
        {error && !course ? <ErrorState title="Không thể mở môn học" description={error} /> : null}
        {!course && !error ? <LoadingSkeleton lines={7} /> : null}
        {course ? (
          <>
            <header className={`course-detail-hero course-theme--${course.themeKey}`}>
              <div>
                <Link className="text-link" to={basePath}>← Thư viện môn học</Link>
                <p className="academy-kicker">{course.code}</p>
                <h2>{course.name}</h2>
                <p>{course.description || 'Môn học chưa có mô tả.'}</p>
                <div className="course-detail-hero__badges">
                  <span>{course.semester || 'Chưa đặt học kỳ'}</span>
                  <span>{course.academicYear || 'Chưa đặt năm học'}</span>
                  <span>{course.isArchived ? 'Đã lưu trữ' : 'Đang hoạt động'}</span>
                </div>
              </div>
              <div className="course-seal" aria-hidden="true"><span>{course.code.slice(0, 2)}</span><small>ARCANA</small></div>
            </header>

            {error ? <p className="form-error" role="alert">{error}</p> : null}

            <section className="identity-grid course-stat-grid">
              <article className="rune-card"><span className="rune-card__symbol">◇</span><h3>Sinh viên</h3><p>{course.studentCount}</p></article>
              <article className="rune-card"><span className="rune-card__symbol">✦</span><h3>Bài tập</h3><p>{course.assignmentCount}</p></article>
              <article className="rune-card"><span className="rune-card__symbol">✧</span><h3>Giảng viên</h3><p>{course.teacherName}</p></article>
            </section>

            <section className="course-action-grid">
              {course.canManage ? (
                <Link className="course-action-card" to={`${basePath}/${course.id}/students`}>
                  <span>♢</span><div><h3>Quản lý sinh viên</h3><p>Thêm thủ công, tìm kiếm, loại khỏi lớp hoặc import Excel.</p></div>
                </Link>
              ) : null}
              {user.role === 'Teacher' && course.canManage ? (
                <Link className="course-action-card" to={`/teacher/courses/${course.id}/edit`}>
                  <span>✎</span><div><h3>Chỉnh sửa môn học</h3><p>Cập nhật mã, tên, học kỳ, mô tả và ấn ký.</p></div>
                </Link>
              ) : null}
              <article className="course-action-card course-action-card--quiet">
                <span>✦</span><div><h3>Không gian bài tập</h3><p>Module Assignment sẽ được nối vào học phần này ở Phase 4.</p></div>
              </article>
            </section>

            {user.role === 'Teacher' && course.canManage ? (
              <section className="danger-zone">
                <div><h3>Quản trị vòng đời</h3><p>Lưu trữ để giữ dữ liệu, hoặc chỉ xóa khi môn chưa có sinh viên.</p></div>
                <div>
                  <button type="button" disabled={working} onClick={toggleArchive}>
                    {course.isArchived ? 'Khôi phục môn học' : 'Lưu trữ môn học'}
                  </button>
                  <button className="danger-button" type="button" disabled={working} onClick={deleteCourse}>Xóa môn học</button>
                </div>
              </section>
            ) : null}
          </>
        ) : null}
      </div>
    </AcademyShell>
  );
}
