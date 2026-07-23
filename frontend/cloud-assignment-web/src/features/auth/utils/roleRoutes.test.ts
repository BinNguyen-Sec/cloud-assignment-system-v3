import { describe, expect, it } from 'vitest';
import { getRoleHomeRoute } from './roleRoutes';

describe('getRoleHomeRoute', () => {
  it('returns a dedicated route for every role', () => {
    expect(getRoleHomeRoute('Admin')).toBe('/admin/overview');
    expect(getRoleHomeRoute('Teacher')).toBe('/teacher/overview');
    expect(getRoleHomeRoute('Student')).toBe('/student/overview');
  });
});
