namespace HrManagement.Api.Shared.DTOs;

/// <summary>
/// Represents a paged result for list endpoints with pagination.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalItems,
    int Page,
    int PageSize
)
{
    /// <summary>
    /// Total number of pages based on total items and page size.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Creates an empty paged result.
    /// </summary>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 10)
        => new(Enumerable.Empty<T>(), 0, page, pageSize);

    /// <summary>
    /// Creates a paged result from a list with manual pagination info.
    /// </summary>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalItems, int page, int pageSize)
        => new(items, totalItems, page, pageSize);
}
