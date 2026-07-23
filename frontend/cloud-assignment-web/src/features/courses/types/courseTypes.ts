import type { PagedResponse } from '../../../types/api';

export type CourseThemeKey =
  | 'astral'
  | 'alchemy'
  | 'runes'
  | 'celestial'
  | 'botany'
  | 'crystal';

export interface CourseSummary {
  id: string;
  code: string;
  name: string;
  description: string | null;
  semester: string | null;
  academicYear: string | null;
  teacherId: string;
  teacherName: string;
  isArchived: boolean;
  themeKey: CourseThemeKey;
  studentCount: number;
  assignmentCount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CourseDetail extends CourseSummary {
  canManage: boolean;
}

export interface CourseMutationPayload {
  code: string;
  name: string;
  description?: string | null;
  semester?: string | null;
  academicYear?: string | null;
  themeKey: CourseThemeKey;
}

export interface CourseQuery {
  q?: string;
  sort?: 'updatedAt' | 'createdAt' | 'name' | 'code' | 'studentCount';
  direction?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
  semester?: string;
  academicYear?: string;
  archived?: boolean;
}

export type CoursePage = PagedResponse<CourseSummary>;

export interface CourseStudent {
  userId: string;
  studentCode: string | null;
  fullName: string;
  email: string;
  enrollmentSource: 'Manual' | 'Excel';
  enrolledAtUtc: string;
}

export interface StudentQuery {
  q?: string;
  sort?: 'fullName' | 'studentCode' | 'email' | 'enrolledAt';
  direction?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

export type CourseStudentPage = PagedResponse<CourseStudent>;
