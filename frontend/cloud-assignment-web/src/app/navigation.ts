import type { UserRole } from '../features/auth/types/authTypes';

export interface NavigationItem {
  label: string;
  description: string;
  href: string;
  symbol: string;
}

const navigationByRole: Record<UserRole, NavigationItem[]> = {
  Admin: [
    { label: 'Tổng quan', description: 'Trạng thái quản trị học viện', href: '/admin/overview', symbol: '✦' },
    { label: 'Môn học', description: 'Toàn bộ học phần', href: '/admin/courses', symbol: '◇' },
    { label: 'Lịch sử import', description: 'Các batch Excel', href: '/admin/import-history', symbol: '⌁' },
  ],
  Teacher: [
    { label: 'Tổng quan', description: 'Không gian giảng viên', href: '/teacher/overview', symbol: '✧' },
    { label: 'Môn học', description: 'Thư viện học phần', href: '/teacher/courses', symbol: '◇' },
    { label: 'Lịch sử import', description: 'Theo dõi Excel', href: '/teacher/import-history', symbol: '⌁' },
  ],
  Student: [
    { label: 'Tổng quan', description: 'Không gian sinh viên', href: '/student/overview', symbol: '◇' },
    { label: 'Môn học', description: 'Các học phần đã ghi danh', href: '/student/courses', symbol: '✦' },
  ],
};

export function getNavigationForRole(role: UserRole): NavigationItem[] {
  return navigationByRole[role];
}
