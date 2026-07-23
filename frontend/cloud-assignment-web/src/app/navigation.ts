import type { UserRole } from '../features/auth/types/authTypes';

export interface NavigationItem {
  label: string;
  description: string;
  href: string;
  symbol: string;
}

const navigationByRole: Record<UserRole, NavigationItem[]> = {
  Admin: [
    {
      label: 'Tổng quan',
      description: 'Trạng thái quản trị học viện',
      href: '/admin/overview',
      symbol: '✦',
    },
  ],
  Teacher: [
    {
      label: 'Tổng quan',
      description: 'Không gian giảng viên',
      href: '/teacher/overview',
      symbol: '✧',
    },
  ],
  Student: [
    {
      label: 'Tổng quan',
      description: 'Không gian sinh viên',
      href: '/student/overview',
      symbol: '◇',
    },
  ],
};

export function getNavigationForRole(role: UserRole): NavigationItem[] {
  return navigationByRole[role];
}
