interface PaginationProps {
  page: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
}

export function Pagination({
  page,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <nav className="pagination" aria-label="Phân trang">
      <button type="button" disabled={!hasPreviousPage} onClick={() => onPageChange(page - 1)}>
        ← Trước
      </button>
      <span>Trang {page} / {totalPages}</span>
      <button type="button" disabled={!hasNextPage} onClick={() => onPageChange(page + 1)}>
        Sau →
      </button>
    </nav>
  );
}
