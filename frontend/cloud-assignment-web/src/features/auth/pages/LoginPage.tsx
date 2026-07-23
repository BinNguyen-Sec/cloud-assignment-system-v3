import { useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../context/AuthContext';
import { getRoleHomeRoute } from '../utils/roleRoutes';

const demoAccounts = [
  { label: 'Admin', email: 'admin@arcana.local', symbol: '✦' },
  { label: 'Teacher', email: 'teacher@arcana.local', symbol: '✧' },
  { label: 'Student', email: 'student@arcana.local', symbol: '◇' },
];

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState('teacher@arcana.local');
  const [password, setPassword] = useState('Arcana@2026!');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      const user = await login({ email, password });
      const returnTo = searchParams.get('returnTo');
      const destination = returnTo?.startsWith('/')
        ? returnTo
        : user.mustChangePassword
          ? '/change-password'
          : getRoleHomeRoute(user.role);
      navigate(destination, { replace: true });
    } catch (caught: unknown) {
      setError(caught instanceof ApiError ? caught.message : 'Không thể kết nối đến học viện.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-story" aria-label="Giới thiệu học viện">
        <div className="login-story__crest" aria-hidden="true">✦</div>
        <p className="academy-kicker">Cloud Assignment System V3</p>
        <h1>Chào mừng trở lại Học viện Arcana.</h1>
        <p>
          Một không gian học tập cloud-native, nơi mỗi học phần là một lĩnh vực phép thuật
          và mọi bài nộp đều được bảo vệ bởi kiến trúc sạch.
        </p>
        <div className="constellation" aria-hidden="true">
          <span>✦</span><span>·</span><span>✧</span><span>·</span><span>◇</span>
        </div>
      </section>

      <section className="login-panel">
        <div className="login-card">
          <p className="academy-kicker">Cổng xác thực</p>
          <h2>Đăng nhập vào học viện</h2>
          <p className="login-card__intro">Sử dụng tài khoản được cấp theo đúng vai trò.</p>

          <form onSubmit={handleSubmit} className="magic-form">
            <label>
              <span>Email</span>
              <input
                autoComplete="email"
                type="email"
                value={email}
                onChange={(event: ChangeEvent<HTMLInputElement>) => setEmail(event.target.value)}
                required
              />
            </label>
            <label>
              <span>Mật khẩu</span>
              <input
                autoComplete="current-password"
                type="password"
                value={password}
                onChange={(event: ChangeEvent<HTMLInputElement>) => setPassword(event.target.value)}
                required
              />
            </label>

            {error ? <p className="form-error" role="alert">{error}</p> : null}

            <button className="primary-magic-button" type="submit" disabled={submitting}>
              {submitting ? 'Đang mở cổng…' : 'Bước vào học viện'}
            </button>
          </form>

          <div className="demo-accounts">
            <span>Tài khoản demo local</span>
            <div>
              {demoAccounts.map((account) => (
                <button
                  type="button"
                  key={account.email}
                  onClick={() => {
                    setEmail(account.email);
                    setPassword('Arcana@2026!');
                    setError(null);
                  }}
                >
                  <b aria-hidden="true">{account.symbol}</b>
                  {account.label}
                </button>
              ))}
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}
