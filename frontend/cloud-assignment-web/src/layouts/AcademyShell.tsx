import type { PropsWithChildren } from 'react';
import { NavLink } from 'react-router-dom';
import type { NavigationItem } from '../app/navigation';

interface AcademyShellProps extends PropsWithChildren {
  navigation: NavigationItem[];
}

export function AcademyShell({ navigation, children }: AcademyShellProps) {
  return (
    <div className="academy-shell">
      <aside className="academy-sidebar" aria-label="Điều hướng chính">
        <div className="academy-crest" aria-hidden="true">
          <span className="academy-crest__star">✦</span>
        </div>

        <div className="academy-brand">
          <p className="academy-brand__eyebrow">Cloud Assignment</p>
          <h1 className="academy-brand__title">Arcana Academy</h1>
          <p className="academy-brand__subtitle">V3 Foundation</p>
        </div>

        <nav className="academy-nav">
          {navigation.map((item, index) => {
            const disabled = index > 0;

            return disabled ? (
              <div
                className="academy-nav__item academy-nav__item--disabled"
                key={item.label}
                aria-disabled="true"
                title={item.description}
              >
                <span className="academy-nav__symbol" aria-hidden="true">
                  {item.symbol}
                </span>
                <span>
                  <strong>{item.label}</strong>
                  <small>Chưa mở</small>
                </span>
              </div>
            ) : (
              <NavLink className="academy-nav__item" to={item.href} key={item.label}>
                <span className="academy-nav__symbol" aria-hidden="true">
                  {item.symbol}
                </span>
                <span>
                  <strong>{item.label}</strong>
                  <small>{item.description}</small>
                </span>
              </NavLink>
            );
          })}
        </nav>

        <div className="academy-sidebar__footer">
          <span className="status-orb" aria-hidden="true" />
          <span>Foundation specification frozen</span>
        </div>
      </aside>

      <main className="academy-main">{children}</main>
    </div>
  );
}
