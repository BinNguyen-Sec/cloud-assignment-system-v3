import { useState, type FormEvent } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getNavigationForRole } from '../../../app/navigation';
import { AcademyShell } from '../../../layouts/AcademyShell';
import { ApiError } from '../../../services/api/apiClient';
import { useAuth } from '../../auth/context/AuthContext';
import { courseApi } from '../../courses/api/courseApi';
import { ImportPreviewTable } from '../components/ImportPreviewTable';
import type { StudentImportConfirm, StudentImportPreview } from '../types/studentImportTypes';

export function StudentImportPage() {
  const { courseId } = useParams();
  const { user, accessToken } = useAuth();
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<StudentImportPreview | null>(null);
  const [result, setResult] = useState<StudentImportConfirm | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [working, setWorking] = useState(false);

  if (!user || user.role !== 'Teacher' || !courseId) return null;

  async function previewFile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!accessToken || !file) return;
    setWorking(true); setError(null); setResult(null);
    try { setPreview(await courseApi.previewImport(accessToken, courseId!, file)); }
    catch (caught: unknown) { setError(caught instanceof ApiError ? caught.message : 'Không thể đọc file Excel.'); }
    finally { setWorking(false); }
  }

  async function confirmImport() {
    if (!accessToken || !preview) return;
    setWorking(true); setError(null);
    try { setResult(await courseApi.confirmImport(accessToken, courseId!, preview.batchId)); }
    catch (caught: unknown) { setError(caught instanceof ApiError ? caught.message : 'Không thể xác nhận import.'); }
    finally { setWorking(false); }
  }

  return (
    <AcademyShell navigation={getNavigationForRole(user.role)}>
      <div className="academy-page">
        <header className="page-heading"><div><Link className="text-link" to={`/teacher/courses/${courseId}/students`}>← Danh sách sinh viên</Link><p className="academy-kicker">Import Ritual</p><h2>Thêm sinh viên bằng Excel</h2><p>Upload → Preview → Confirm → Result. Không có dòng nào được thêm trước khi ông xác nhận.</p></div><button className="magic-button magic-button--secondary" type="button" onClick={() => accessToken && courseApi.downloadImportTemplate(accessToken, courseId)}>Tải file mẫu</button></header>

        <ol className="import-steps" aria-label="Tiến trình import">
          <li className={!preview ? 'is-active' : 'is-complete'}><span>1</span>Upload</li><li className={preview && !result ? 'is-active' : result ? 'is-complete' : ''}><span>2</span>Preview</li><li className={result ? 'is-complete' : ''}><span>3</span>Confirm</li><li className={result ? 'is-active' : ''}><span>4</span>Result</li>
        </ol>

        {error ? <p className="form-error" role="alert">{error}</p> : null}

        {!preview ? (
          <form className="import-dropzone" onSubmit={previewFile}>
            <span aria-hidden="true">✦</span><h3>Đặt danh sách lớp vào cổng nhập liệu</h3><p>Chỉ nhận .xlsx, tối đa 5 MB và 1.000 dòng.</p>
            <input id="import-file" type="file" accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" onChange={(event) => setFile(event.target.files?.[0] ?? null)} />
            <label className="magic-button magic-button--secondary" htmlFor="import-file">{file ? file.name : 'Chọn file Excel'}</label>
            <button className="magic-button" disabled={!file || working}>{working ? 'Đang phân tích…' : 'Tạo bản xem trước'}</button>
          </form>
        ) : null}

        {preview && !result ? (
          <section className="import-preview-panel">
            <div className="import-summary-grid"><article><strong>{preview.totalRows}</strong><span>Tổng dòng</span></article><article className="is-valid"><strong>{preview.validRows}</strong><span>Hợp lệ</span></article><article className="is-invalid"><strong>{preview.invalidRows}</strong><span>Cần chú ý</span></article><article><strong>{formatExpiry(preview.expiresAtUtc)}</strong><span>Hết hạn preview</span></article></div>
            <ImportPreviewTable rows={preview.rows} />
            <div className="form-actions"><button className="magic-button magic-button--ghost" type="button" onClick={() => { setPreview(null); setFile(null); }}>Chọn file khác</button><button className="magic-button" type="button" disabled={working || preview.validRows === 0} onClick={confirmImport}>{working ? 'Đang xác nhận…' : `Import ${preview.validRows} dòng hợp lệ`}</button></div>
          </section>
        ) : null}

        {result ? (
          <section className="import-result-panel"><div className="magic-success-seal" aria-hidden="true">✓</div><h3>Import đã hoàn tất</h3><p>{result.importedRows} sinh viên đã được thêm, {result.skippedRows} dòng được bỏ qua.</p><div className="form-actions"><button className="magic-button magic-button--secondary" type="button" onClick={() => accessToken && courseApi.downloadImportReport(accessToken, courseId, result.batchId)}>Tải báo cáo kết quả</button><Link className="magic-button" to={`/teacher/courses/${courseId}/students`}>Xem danh sách lớp</Link></div><ImportPreviewTable rows={result.rows} /></section>
        ) : null}
      </div>
    </AcademyShell>
  );
}

function formatExpiry(value: string) { return new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit' }).format(new Date(value)); }
