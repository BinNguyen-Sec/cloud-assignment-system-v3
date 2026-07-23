import { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { getNavigationForRole } from '../../../app/navigation';
import { EmptyState } from '../../../components/feedback/EmptyState';
import { ErrorState } from '../../../components/feedback/ErrorState';
import { LoadingSkeleton } from '../../../components/feedback/LoadingSkeleton';
import { Pagination } from '../../../components/navigation/Pagination';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../../auth/context/AuthContext';
import { courseApi } from '../api/courseApi';
import { CourseCard } from '../components/CourseCard';
import type { CoursePage, CourseQuery } from '../types/courseTypes';
import { readCourseQuery, writeCourseQuery } from '../utils/courseQueryState';

export function CourseLibraryPage() {
  const { user, accessToken } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const query = useMemo(() => readCourseQuery(searchParams), [searchParams]);
  const [draftSearch, setDraftSearch] = useState(query.q ?? '');
  const [pageData, setPageData] = useState<CoursePage | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => setDraftSearch(query.q ?? ''), [query.q]);

  useEffect(() => {
    if (!accessToken) return;
    let active = true;
    setLoading(true);
    setError(null);
    courseApi.list(accessToken, query)
      .then((result) => {
        if (active) setPageData(result);
      })
      .catch((caught: unknown) => {
        if (active) setError(caught instanceof ApiError ? caught.message : 'Không thể tải thư viện môn học.');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => { active = false; };
  }, [accessToken, query]);

  if (!user) return null;

  function updateQuery(patch: Partial<CourseQuery>, resetPage = true) {
    setSearchParams(writeCourseQuery({ ...query, ...patch, page: resetPage ? 1 : patch.page ?? query.page }));
  }

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page course-library-page">
        <header className="academy-hero course-library-hero">
          <div>
            <p className="academy-kicker">Course Library</p>
            <h2>Thư viện học phần phép thuật</h2>
            <p>Tìm, sắp xếp và mở từng môn học trong một không gian riêng biệt.</p>
          </div>
          {user.role === 'Teacher' ? (
            <Link className="magic-button" to="/teacher/courses/new">+ Khai mở môn học</Link>
          ) : null}
        </header>

        <section className="course-command-bar" aria-label="Tìm kiếm và sắp xếp môn học">
          <form
            className="magic-search"
            onSubmit={(event) => {
              event.preventDefault();
              updateQuery({ q: draftSearch.trim() });
            }}
          >
            <label htmlFor="course-search">Tìm môn học</label>
            <div>
              <input
                id="course-search"
                value={draftSearch}
                onChange={(event) => setDraftSearch(event.target.value)}
                placeholder="Tên, mã môn, học kỳ hoặc giảng viên…"
              />
              <button type="submit">Tìm</button>
            </div>
          </form>

          <label>
            Sắp xếp
            <select value={query.sort} onChange={(event) => updateQuery({ sort: event.target.value as CourseQuery['sort'] })}>
              <option value="updatedAt">Mới cập nhật</option>
              <option value="createdAt">Ngày tạo</option>
              <option value="name">Tên môn</option>
              <option value="code">Mã môn</option>
              <option value="studentCount">Số sinh viên</option>
            </select>
          </label>
          <label>
            Thứ tự
            <select value={query.direction} onChange={(event) => updateQuery({ direction: event.target.value as 'asc' | 'desc' })}>
              <option value="desc">Giảm dần</option>
              <option value="asc">Tăng dần</option>
            </select>
          </label>
          <label>
            Trạng thái
            <select
              value={query.archived ? 'archived' : 'active'}
              onChange={(event) => updateQuery({ archived: event.target.value === 'archived' })}
            >
              <option value="active">Đang hoạt động</option>
              <option value="archived">Đã lưu trữ</option>
            </select>
          </label>
        </section>

        {loading ? <LoadingSkeleton /> : null}
        {error ? <ErrorState title="Không thể mở thư viện" description={error} /> : null}
        {!loading && !error && pageData?.items.length === 0 ? (
          <EmptyState
            title={query.q ? 'Không tìm thấy môn học phù hợp' : 'Thư viện đang trống'}
            description={user.role === 'Teacher' ? 'Hãy tạo môn học đầu tiên hoặc thay đổi bộ lọc.' : 'Bạn chưa được ghi danh vào môn học nào.'}
          />
        ) : null}

        {pageData && pageData.items.length > 0 ? (
          <>
            <div className="course-grid">
              {pageData.items.map((course) => <CourseCard key={course.id} course={course} role={user.role} />)}
            </div>
            <Pagination
              page={pageData.page}
              totalPages={pageData.totalPages}
              hasPreviousPage={pageData.hasPreviousPage}
              hasNextPage={pageData.hasNextPage}
              onPageChange={(page) => updateQuery({ page }, false)}
            />
          </>
        ) : null}
      </div>
    </AcademyShell>
  );
}
