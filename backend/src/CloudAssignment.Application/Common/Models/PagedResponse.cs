namespace CloudAssignment.Application.Common.Models;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalItems)
{
    public long TotalPages => TotalItems == 0
        ? 0
        : (long)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
