import type { UserRole } from '../types/authTypes';

const roleRoutes: Record<UserRole, string> = {
  Admin: '/admin/overview',
  Teacher: '/teacher/overview',
  Student: '/student/overview',
};

export function getRoleHomeRoute(role: UserRole): string {
  return roleRoutes[role];
}
