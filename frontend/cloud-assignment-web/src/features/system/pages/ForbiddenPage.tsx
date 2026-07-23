import { Link } from 'react-router-dom';
import { useAuth } from '../../auth/context/AuthContext';
import { getRoleHomeRoute } from '../../auth/utils/roleRoutes';

export function ForbiddenPage() {
  const { user } = useAuth();
  return (
    <main className="centered-state">
      <div className="magic-seal magic-seal--small" aria-hidden="true"><span>!</span><small>403</small></div>
      <h1>Khu vực này không thuộc quyền của bạn.</h1>
      <p>Backend đã từ chối yêu cầu theo role policy.</p>
      <Link className="text-link" to={user ? getRoleHomeRoute(user.role) : '/login'}>Trở về không gian của tôi</Link>
    </main>
  );
}
