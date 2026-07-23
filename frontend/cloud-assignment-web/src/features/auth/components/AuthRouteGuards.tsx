import type { PropsWithChildren } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import type { UserRole } from '../types/authTypes';
import { getRoleHomeRoute } from '../utils/roleRoutes';

export function PublicOnlyRoute({ children }: PropsWithChildren) {
  const { status, user } = useAuth();
  if (status === 'loading') return <AuthenticationLoader />;
  if (status === 'authenticated' && user) {
    return <Navigate replace to={getRoleHomeRoute(user.role)} />;
  }

  return children;
}

interface RequireAuthProps extends PropsWithChildren {
  roles?: UserRole[];
}

export function RequireAuth({ roles, children }: RequireAuthProps) {
  const { status, user } = useAuth();
  const location = useLocation();

  if (status === 'loading') return <AuthenticationLoader />;
  if (status !== 'authenticated' || !user) {
    const returnTo = `${location.pathname}${location.search}`;
    return <Navigate replace to={`/login?returnTo=${encodeURIComponent(returnTo)}`} />;
  }

  if (user.mustChangePassword && location.pathname !== '/change-password') {
    return <Navigate replace to="/change-password" />;
  }

  if (roles && !roles.includes(user.role)) {
    return <Navigate replace to="/forbidden" />;
  }

  return children;
}

function AuthenticationLoader() {
  return (
    <main className="auth-loading" aria-live="polite">
      <div className="magic-seal magic-seal--small" aria-hidden="true">
        <span>✦</span>
        <small>SYNC</small>
      </div>
      <p>Đang khôi phục phiên học viện…</p>
    </main>
  );
}
