export interface NavigationItem {
  label: string;
  description: string;
  href: string;
  symbol: string;
}

export const foundationNavigation: NavigationItem[] = [
  {
    label: 'Nền tảng',
    description: 'Kiến trúc và trạng thái hệ thống',
    href: '/',
    symbol: '✦',
  },
  {
    label: 'Học phần',
    description: 'Sẽ mở ở Phase Course Management',
    href: '/',
    symbol: '◈',
  },
  {
    label: 'Bài tập',
    description: 'Sẽ mở ở Phase Assignment',
    href: '/',
    symbol: '✧',
  },
  {
    label: 'Bài nộp',
    description: 'Sẽ mở ở Phase Submission',
    href: '/',
    symbol: '◇',
  },
];
