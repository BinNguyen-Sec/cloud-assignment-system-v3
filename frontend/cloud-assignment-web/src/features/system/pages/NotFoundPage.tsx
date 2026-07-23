import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <main className="centered-state">
      <div className="magic-seal magic-seal--small" aria-hidden="true">
        <span>404</span>
      </div>
      <p className="academy-kicker">Lạc khỏi hành lang học viện</p>
      <h1>Trang này không tồn tại.</h1>
      <Link className="text-link" to="/">
        Quay về nền tảng
      </Link>
    </main>
  );
}
