import { useEffect, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getNavigationForRole } from '../../../app/navigation';
import { LoadingSkeleton } from '../../../components/feedback/LoadingSkeleton';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../../auth/context/AuthContext';
import { courseApi } from '../api/courseApi';
import type { CourseMutationPayload, CourseThemeKey } from '../types/courseTypes';

const emptyForm: CourseMutationPayload = {
  code: '',
  name: '',
  description: '',
  semester: '',
  academicYear: '',
  themeKey: 'astral',
};

const themes: { value: CourseThemeKey; label: string }[] = [
  { value: 'astral', label: 'Tinh tú' },
  { value: 'alchemy', label: 'Giả kim' },
  { value: 'runes', label: 'Cổ ngữ Rune' },
  { value: 'celestial', label: 'Thiên giới' },
  { value: 'botany', label: 'Thảo mộc' },
  { value: 'crystal', label: 'Pha lê' },
];

export function CourseFormPage() {
  const { courseId } = useParams();
  const editing = Boolean(courseId);
  const { user, accessToken } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState<CourseMutationPayload>(emptyForm);
  const [loading, setLoading] = useState(editing);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  useEffect(() => {
    if (!editing || !courseId || !accessToken) return;
    courseApi.get(accessToken, courseId)
      .then((course) => {
        setForm({
          code: course.code,
          name: course.name,
          description: course.description ?? '',
          semester: course.semester ?? '',
          academicYear: course.academicYear ?? '',
          themeKey: course.themeKey,
        });
      })
      .catch((caught: unknown) => setError(caught instanceof ApiError ? caught.message : 'Không thể tải môn học.'))
      .finally(() => setLoading(false));
  }, [accessToken, courseId, editing]);

  if (!user || user.role !== 'Teacher') return null;

  function update<K extends keyof CourseMutationPayload>(key: K, value: CourseMutationPayload[K]) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!accessToken) return;
    setSaving(true);
    setError(null);
    setFieldErrors({});
    try {
      const saved = editing && courseId
        ? await courseApi.update(accessToken, courseId, form)
        : await courseApi.create(accessToken, form);
      navigate(`/teacher/courses/${saved.id}`, { replace: true });
    } catch (caught: unknown) {
      if (caught instanceof ApiError) {
        setError(caught.message);
        setFieldErrors(caught.problem.errors ?? {});
      } else {
        setError('Không thể lưu môn học.');
      }
    } finally {
      setSaving(false);
    }
  }

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page narrow-page">
        <header className="page-heading">
          <div>
            <p className="academy-kicker">Teacher · Course Management</p>
            <h2>{editing ? 'Hiệu chỉnh học phần' : 'Khai mở học phần mới'}</h2>
            <p>Mọi thông tin được lưu trọn vẹn trong một cấu trúc môn học riêng biệt.</p>
          </div>
          <Link className="text-link" to="/teacher/courses">← Thư viện môn học</Link>
        </header>

        {loading ? <LoadingSkeleton lines={6} /> : (
          <form className="arcane-form course-form" onSubmit={handleSubmit}>
            {error ? <p className="form-error" role="alert">{error}</p> : null}
            <div className="form-grid form-grid--two">
              <label>
                Mã môn học
                <input value={form.code} maxLength={40} onChange={(event) => update('code', event.target.value)} />
                <FieldError errors={fieldErrors.code} />
              </label>
              <label>
                Tên môn học
                <input value={form.name} maxLength={180} onChange={(event) => update('name', event.target.value)} />
                <FieldError errors={fieldErrors.name} />
              </label>
              <label>
                Học kỳ
                <input value={form.semester ?? ''} maxLength={30} placeholder="Học kỳ 1" onChange={(event) => update('semester', event.target.value)} />
                <FieldError errors={fieldErrors.semester} />
              </label>
              <label>
                Năm học
                <input value={form.academicYear ?? ''} maxLength={20} placeholder="2026–2027" onChange={(event) => update('academicYear', event.target.value)} />
                <FieldError errors={fieldErrors.academicYear} />
              </label>
            </div>
            <label>
              Mô tả
              <textarea value={form.description ?? ''} maxLength={4000} rows={6} onChange={(event) => update('description', event.target.value)} />
              <FieldError errors={fieldErrors.description} />
            </label>
            <fieldset className="theme-picker">
              <legend>Ấn ký học phần</legend>
              <div>
                {themes.map((theme) => (
                  <label className={`theme-choice course-theme--${theme.value}`} key={theme.value}>
                    <input
                      type="radio"
                      name="themeKey"
                      value={theme.value}
                      checked={form.themeKey === theme.value}
                      onChange={() => update('themeKey', theme.value)}
                    />
                    <span aria-hidden="true">✦</span>
                    {theme.label}
                  </label>
                ))}
              </div>
              <FieldError errors={fieldErrors.themeKey} />
            </fieldset>
            <div className="form-actions">
              <Link className="magic-button magic-button--ghost" to="/teacher/courses">Hủy</Link>
              <button className="magic-button" type="submit" disabled={saving}>
                {saving ? 'Đang ghi vào thư viện…' : editing ? 'Lưu thay đổi' : 'Tạo môn học'}
              </button>
            </div>
          </form>
        )}
      </div>
    </AcademyShell>
  );
}

function FieldError({ errors }: { errors?: string[] }) {
  return errors?.length ? <small className="field-error">{errors[0]}</small> : null;
}
