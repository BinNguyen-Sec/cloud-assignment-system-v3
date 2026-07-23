import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth, PublicOnlyRoute } from '../features/auth/components/AuthRouteGuards';
import { AuthProvider, useAuth } from '../features/auth/context/AuthContext';
import { ChangePasswordPage } from '../features/auth/pages/ChangePasswordPage';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { getRoleHomeRoute } from '../features/auth/utils/roleRoutes';
import { ForbiddenPage } from '../features/system/pages/ForbiddenPage';
import { NotFoundPage } from '../features/system/pages/NotFoundPage';
import { RoleOverviewPage } from '../features/system/pages/RoleOverviewPage';

export function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/" element={<RootRedirect />} />
        <Route path="/login" element={<PublicOnlyRoute><LoginPage /></PublicOnlyRoute>} />
        <Route path="/change-password" element={<RequireAuth><ChangePasswordPage /></RequireAuth>} />
        <Route path="/admin/overview" element={<RequireAuth roles={['Admin']}><RoleOverviewPage /></RequireAuth>} />
        <Route path="/teacher/overview" element={<RequireAuth roles={['Teacher']}><RoleOverviewPage /></RequireAuth>} />
        <Route path="/student/overview" element={<RequireAuth roles={['Student']}><RoleOverviewPage /></RequireAuth>} />
        <Route path="/forbidden" element={<RequireAuth><ForbiddenPage /></RequireAuth>} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </AuthProvider>
  );
}

function RootRedirect() {
  const { status, user } = useAuth();
  if (status === 'loading') return null;
  return <Navigate replace to={user ? getRoleHomeRoute(user.role) : '/login'} />;
}
