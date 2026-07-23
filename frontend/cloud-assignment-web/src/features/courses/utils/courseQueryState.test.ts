import { describe, expect, it } from 'vitest';
import { readCourseQuery, writeCourseQuery } from './courseQueryState';

describe('course query state', () => {
  it('uses safe defaults for an empty URL', () => {
    const query = readCourseQuery(new URLSearchParams());
    expect(query.page).toBe(1);
    expect(query.pageSize).toBe(20);
    expect(query.sort).toBe('updatedAt');
    expect(query.archived).toBe(false);
  });

  it('round trips supported values', () => {
    const params = writeCourseQuery({ q: 'cloud', page: 2, sort: 'name', direction: 'asc' });
    const query = readCourseQuery(params);
    expect(query.q).toBe('cloud');
    expect(query.page).toBe(2);
    expect(query.sort).toBe('name');
    expect(query.direction).toBe('asc');
  });
});
