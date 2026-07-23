import type { CourseQuery } from '../types/courseTypes';

export function readCourseQuery(params: URLSearchParams): Required<Pick<CourseQuery, 'page' | 'pageSize' | 'sort' | 'direction'>> & CourseQuery {
  const archivedValue = params.get('archived');
  return {
    q: params.get('q') ?? '',
    sort: parseSort(params.get('sort')),
    direction: params.get('direction') === 'asc' ? 'asc' : 'desc',
    page: positiveInteger(params.get('page'), 1),
    pageSize: pageSize(params.get('pageSize')),
    semester: params.get('semester') ?? '',
    academicYear: params.get('academicYear') ?? '',
    archived: archivedValue === null ? false : archivedValue === 'true',
  };
}

export function writeCourseQuery(query: CourseQuery): URLSearchParams {
  const params = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value));
    }
  });
  return params;
}

function positiveInteger(value: string | null, fallback: number): number {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : fallback;
}

function pageSize(value: string | null): number {
  const parsed = positiveInteger(value, 20);
  return [10, 20, 50].includes(parsed) ? parsed : 20;
}

function parseSort(value: string | null): NonNullable<CourseQuery['sort']> {
  const allowed: NonNullable<CourseQuery['sort']>[] = [
    'updatedAt',
    'createdAt',
    'name',
    'code',
    'studentCount',
  ];
  return allowed.includes(value as NonNullable<CourseQuery['sort']>)
    ? (value as NonNullable<CourseQuery['sort']>)
    : 'updatedAt';
}
