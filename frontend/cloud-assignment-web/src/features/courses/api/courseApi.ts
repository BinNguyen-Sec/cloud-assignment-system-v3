import { apiDownload, apiRequest } from '../../../services/api/apiClient';
import type {
  CourseDetail,
  CourseMutationPayload,
  CoursePage,
  CourseQuery,
  CourseStudent,
  CourseStudentPage,
  StudentQuery,
} from '../types/courseTypes';
import type {
  StudentImportConfirm,
  StudentImportHistoryPage,
  StudentImportPreview,
} from '../../student-imports/types/studentImportTypes';

function toQueryString(values: object): string {
  const params = new URLSearchParams();
  Object.entries(values as Record<string, string | number | boolean | undefined | null>).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value));
    }
  });
  const query = params.toString();
  return query ? `?${query}` : '';
}

export const courseApi = {
  list(accessToken: string, query: CourseQuery): Promise<CoursePage> {
    return apiRequest<CoursePage>(`/courses${toQueryString(query)}`, { accessToken });
  },

  get(accessToken: string, courseId: string): Promise<CourseDetail> {
    return apiRequest<CourseDetail>(`/courses/${courseId}`, { accessToken });
  },

  create(accessToken: string, payload: CourseMutationPayload): Promise<CourseDetail> {
    return apiRequest<CourseDetail>('/courses', {
      method: 'POST',
      accessToken,
      body: payload,
    });
  },

  update(
    accessToken: string,
    courseId: string,
    payload: CourseMutationPayload,
  ): Promise<CourseDetail> {
    return apiRequest<CourseDetail>(`/courses/${courseId}`, {
      method: 'PUT',
      accessToken,
      body: payload,
    });
  },

  archive(accessToken: string, courseId: string): Promise<void> {
    return apiRequest<void>(`/courses/${courseId}/archive`, { method: 'POST', accessToken });
  },

  restore(accessToken: string, courseId: string): Promise<void> {
    return apiRequest<void>(`/courses/${courseId}/restore`, { method: 'POST', accessToken });
  },

  remove(accessToken: string, courseId: string): Promise<void> {
    return apiRequest<void>(`/courses/${courseId}`, { method: 'DELETE', accessToken });
  },

  listStudents(
    accessToken: string,
    courseId: string,
    query: StudentQuery,
  ): Promise<CourseStudentPage> {
    return apiRequest<CourseStudentPage>(
      `/courses/${courseId}/students${toQueryString(query)}`,
      { accessToken },
    );
  },

  enrollStudent(
    accessToken: string,
    courseId: string,
    email: string,
  ): Promise<CourseStudent> {
    return apiRequest<CourseStudent>(`/courses/${courseId}/students`, {
      method: 'POST',
      accessToken,
      body: { email },
    });
  },

  removeStudent(accessToken: string, courseId: string, studentId: string): Promise<void> {
    return apiRequest<void>(`/courses/${courseId}/students/${studentId}`, {
      method: 'DELETE',
      accessToken,
    });
  },

  async downloadImportTemplate(accessToken: string, courseId: string): Promise<void> {
    const download = await apiDownload(
      `/courses/${courseId}/students/import-template`,
      accessToken,
    );
    saveBlob(download.blob, download.fileName);
  },

  previewImport(
    accessToken: string,
    courseId: string,
    file: File,
  ): Promise<StudentImportPreview> {
    const form = new FormData();
    form.append('file', file);
    return apiRequest<StudentImportPreview>(`/courses/${courseId}/students/import-preview`, {
      method: 'POST',
      accessToken,
      body: form,
    });
  },

  confirmImport(
    accessToken: string,
    courseId: string,
    batchId: string,
  ): Promise<StudentImportConfirm> {
    return apiRequest<StudentImportConfirm>(
      `/courses/${courseId}/students/imports/${batchId}/confirm`,
      { method: 'POST', accessToken },
    );
  },

  listImports(
    accessToken: string,
    query: { page?: number; pageSize?: number; status?: string; courseId?: string },
  ): Promise<StudentImportHistoryPage> {
    return apiRequest<StudentImportHistoryPage>(
      `/student-imports${toQueryString(query)}`,
      { accessToken },
    );
  },

  async downloadImportReport(
    accessToken: string,
    courseId: string,
    batchId: string,
  ): Promise<void> {
    const download = await apiDownload(
      `/courses/${courseId}/students/imports/${batchId}/error-report`,
      accessToken,
    );
    saveBlob(download.blob, download.fileName);
  },
};

function saveBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}
