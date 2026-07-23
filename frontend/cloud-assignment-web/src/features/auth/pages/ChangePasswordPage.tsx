import { useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../context/AuthContext';

export function ChangePasswordPage() {
  const { changePassword } = useAuth();
  const navigate = useNavigate();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await changePassword({ currentPassword, newPassword, confirmNewPassword });
      navigate('/login?passwordChanged=true', { replace: true });
    } catch (caught: unknown) {
      setError(caught instanceof ApiError ? caught.message : 'Không thể đổi mật khẩu.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="standalone-form-page">
      <section className="login-card change-password-card">
        <p className="academy-kicker">Bảo vệ danh tính</p>
        <h1>Đổi mật khẩu</h1>
        <p>Mật khẩu mới cần ít nhất 10 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.</p>
        <form className="magic-form" onSubmit={handleSubmit}>
          <label>
            <span>Mật khẩu hiện tại</span>
            <input type="password" value={currentPassword} onChange={(event: ChangeEvent<HTMLInputElement>) => setCurrentPassword(event.target.value)} required />
          </label>
          <label>
            <span>Mật khẩu mới</span>
            <input type="password" value={newPassword} onChange={(event: ChangeEvent<HTMLInputElement>) => setNewPassword(event.target.value)} required />
          </label>
          <label>
            <span>Xác nhận mật khẩu mới</span>
            <input type="password" value={confirmNewPassword} onChange={(event: ChangeEvent<HTMLInputElement>) => setConfirmNewPassword(event.target.value)} required />
          </label>
          {error ? <p className="form-error" role="alert">{error}</p> : null}
          <button className="primary-magic-button" type="submit" disabled={submitting}>
            {submitting ? 'Đang cập nhật…' : 'Xác nhận mật khẩu mới'}
          </button>
        </form>
      </section>
    </main>
  );
}
