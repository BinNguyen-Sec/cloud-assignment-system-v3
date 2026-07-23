import type { PropsWithChildren } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import type { NavigationItem } from '../app/navigation';
import { useAuth } from '../features/auth/context/AuthContext';

interface AcademyShellProps extends PropsWithChildren {
  navigation: NavigationItem[];
}

export function AcademyShell({ navigation, children }: AcademyShellProps) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  async function handleLogout() {
    await logout();
    navigate('/login', { replace: true });
  }

  return (
    <div className="academy-shell">
      <aside className="academy-sidebar" aria-label="Điều hướng chính">
        <div className="academy-crest" aria-hidden="true">
          <span className="academy-crest__star">✦</span>
        </div>

        <div className="academy-brand">
          <p className="academy-brand__eyebrow">Cloud Assignment</p>
          <h1 className="academy-brand__title">Arcana Academy</h1>
          <p className="academy-brand__subtitle">Modern Magical Learning</p>
        </div>

        <nav className="academy-nav">
          {navigation.map((item) => (
            <NavLink className="academy-nav__item" to={item.href} key={item.href}>
              <span className="academy-nav__symbol" aria-hidden="true">
                {item.symbol}
              </span>
              <span>
                <strong>{item.label}</strong>
                <small>{item.description}</small>
              </span>
            </NavLink>
          ))}
        </nav>

        <div className="academy-user-card">
          <span className="academy-user-card__avatar" aria-hidden="true">
            {user?.fullName.charAt(0).toUpperCase() ?? 'A'}
          </span>
          <div>
            <strong>{user?.fullName}</strong>
            <small>{user?.role}</small>
          </div>
          <button type="button" onClick={handleLogout}>
            Đăng xuất
          </button>
        </div>
      </aside>

      <main className="academy-main">{children}</main>
    </div>
  );
}
