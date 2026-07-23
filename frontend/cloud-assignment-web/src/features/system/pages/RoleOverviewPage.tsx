import { useEffect, useState } from 'react';
import { getNavigationForRole } from '../../../app/navigation';
import { useAuth } from '../../auth/context/AuthContext';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { apiRequest, ApiError } from '../../../services/api/apiClient';

interface OverviewResponse {
  phase: string;
  message: string;
}

export function RoleOverviewPage() {
  const { user, accessToken } = useAuth();
  const [overview, setOverview] = useState<OverviewResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!user || !accessToken) return;
    apiRequest<OverviewResponse>(`/${user.role.toLowerCase()}/overview`, { accessToken })
      .then(setOverview)
      .catch((caught: unknown) => {
        setError(caught instanceof ApiError ? caught.message : 'Không thể tải không gian vai trò.');
      });
  }, [accessToken, user]);

  if (!user) return null;

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page">
        <header className="academy-hero role-hero">
          <div>
            <p className="academy-kicker">Phase 3 · Course Management</p>
            <h2>Xin chào, {user.fullName}.</h2>
            <p>{overview?.message ?? 'Đang đồng bộ không gian học viện của bạn…'}</p>
          </div>
          <div className="magic-seal" aria-label={`${user.role} authenticated`}>
            <span>{user.role.charAt(0)}</span>
            <small>VERIFIED</small>
          </div>
        </header>

        {error ? <p className="form-error overview-error" role="alert">{error}</p> : null}

        <section className="identity-grid" aria-label="Thông tin phiên đăng nhập">
          <article className="rune-card">
            <span className="rune-card__symbol" aria-hidden="true">✦</span>
            <h3>Vai trò đã xác thực</h3>
            <p>{user.role}</p>
          </article>
          <article className="rune-card">
            <span className="rune-card__symbol" aria-hidden="true">@</span>
            <h3>Danh tính học viện</h3>
            <p>{user.email}</p>
          </article>
          <article className="rune-card">
            <span className="rune-card__symbol" aria-hidden="true">✓</span>
            <h3>Trạng thái hệ thống</h3>
            <p>{overview ? `${overview.phase} đã hoạt động` : 'Đang kiểm tra API'}</p>
          </article>
        </section>

        <section className="architecture-panel next-module-panel">
          <div>
            <p className="academy-kicker">Module đang hoạt động</p>
            <h3>Course Library + Excel Import</h3>
            <p>
              Course Library, search, sort, filter, pagination, enrollment thủ công và import Excel
              đã được tách thành các trang nghiệp vụ độc lập.
            </p>
          </div>
          <span className="phase-sigil" aria-hidden="true">✓</span>
        </section>
      </div>
    </AcademyShell>
  );
}
