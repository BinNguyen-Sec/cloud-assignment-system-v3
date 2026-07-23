import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth, PublicOnlyRoute } from '../features/auth/components/AuthRouteGuards';
import { AuthProvider, useAuth } from '../features/auth/context/AuthContext';
import { ChangePasswordPage } from '../features/auth/pages/ChangePasswordPage';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { getRoleHomeRoute } from '../features/auth/utils/roleRoutes';
import { CourseDetailPage } from '../features/courses/pages/CourseDetailPage';
import { CourseFormPage } from '../features/courses/pages/CourseFormPage';
import { CourseLibraryPage } from '../features/courses/pages/CourseLibraryPage';
import { CourseStudentsPage } from '../features/courses/pages/CourseStudentsPage';
import { ImportHistoryPage } from '../features/student-imports/pages/ImportHistoryPage';
import { StudentImportPage } from '../features/student-imports/pages/StudentImportPage';
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
        <Route path="/admin/courses" element={<RequireAuth roles={['Admin']}><CourseLibraryPage /></RequireAuth>} />
        <Route path="/admin/courses/:courseId" element={<RequireAuth roles={['Admin']}><CourseDetailPage /></RequireAuth>} />
        <Route path="/admin/courses/:courseId/students" element={<RequireAuth roles={['Admin']}><CourseStudentsPage /></RequireAuth>} />
        <Route path="/admin/import-history" element={<RequireAuth roles={['Admin']}><ImportHistoryPage /></RequireAuth>} />

        <Route path="/teacher/overview" element={<RequireAuth roles={['Teacher']}><RoleOverviewPage /></RequireAuth>} />
        <Route path="/teacher/courses" element={<RequireAuth roles={['Teacher']}><CourseLibraryPage /></RequireAuth>} />
        <Route path="/teacher/courses/new" element={<RequireAuth roles={['Teacher']}><CourseFormPage /></RequireAuth>} />
        <Route path="/teacher/courses/:courseId" element={<RequireAuth roles={['Teacher']}><CourseDetailPage /></RequireAuth>} />
        <Route path="/teacher/courses/:courseId/edit" element={<RequireAuth roles={['Teacher']}><CourseFormPage /></RequireAuth>} />
        <Route path="/teacher/courses/:courseId/students" element={<RequireAuth roles={['Teacher']}><CourseStudentsPage /></RequireAuth>} />
        <Route path="/teacher/courses/:courseId/students/import" element={<RequireAuth roles={['Teacher']}><StudentImportPage /></RequireAuth>} />
        <Route path="/teacher/import-history" element={<RequireAuth roles={['Teacher']}><ImportHistoryPage /></RequireAuth>} />

        <Route path="/student/overview" element={<RequireAuth roles={['Student']}><RoleOverviewPage /></RequireAuth>} />
        <Route path="/student/courses" element={<RequireAuth roles={['Student']}><CourseLibraryPage /></RequireAuth>} />
        <Route path="/student/courses/:courseId" element={<RequireAuth roles={['Student']}><CourseDetailPage /></RequireAuth>} />

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
