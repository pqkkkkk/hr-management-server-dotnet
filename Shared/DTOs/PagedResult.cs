namespace HrManagement.Api.Shared.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Represents sort information for pagination (matching Spring Boot Page schema).
/// </summary>
public record SortInfo(
    [property: JsonPropertyName("sorted")] bool Sorted = false,
    [property: JsonPropertyName("unsorted")] bool Unsorted = true,
    [property: JsonPropertyName("empty")] bool Empty = true
);

/// <summary>
/// Represents pageable information (matching Spring Boot Pageable schema).
/// </summary>
public record PageableInfo(
    [property: JsonPropertyName("pageNumber")] int PageNumber,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("sort")] SortInfo Sort,
    [property: JsonPropertyName("offset")] long Offset,
    [property: JsonPropertyName("paged")] bool Paged = true,
    [property: JsonPropertyName("unpaged")] bool Unpaged = false
);

/// <summary>
/// Represents a paged result matching Spring Boot's Page schema.
/// This ensures compatibility with the hr-management-client application.
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public record PagedResult<T>
{
    /// <summary>
    /// The content items of the current page.
    /// </summary>
    [JsonPropertyName("content")]
    public IEnumerable<T> Content { get; init; } = Enumerable.Empty<T>();

    /// <summary>
    /// Pageable information including page number, size, sort and offset.
    /// </summary>
    [JsonPropertyName("pageable")]
    public PageableInfo Pageable { get; init; } = null!;

    /// <summary>
    /// Whether this is the last page.
    /// </summary>
    [JsonPropertyName("last")]
    public bool Last { get; init; }

    /// <summary>
    /// Total number of elements across all pages.
    /// </summary>
    [JsonPropertyName("totalElements")]
    public long TotalElements { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    /// <summary>
    /// Whether this is the first page.
    /// </summary>
    [JsonPropertyName("first")]
    public bool First { get; init; }

    /// <summary>
    /// Size of the current page (page size).
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; init; }

    /// <summary>
    /// Current page number (0-indexed to match Spring Boot convention).
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; init; }

    /// <summary>
    /// Sort information for the page.
    /// </summary>
    [JsonPropertyName("sort")]
    public SortInfo Sort { get; init; } = new SortInfo();

    /// <summary>
    /// Number of elements in the current page.
    /// </summary>
    [JsonPropertyName("numberOfElements")]
    public int NumberOfElements { get; init; }

    /// <summary>
    /// Whether the page is empty.
    /// </summary>
    [JsonPropertyName("empty")]
    public bool Empty { get; init; }

    /// <summary>
    /// Creates an empty paged result.
    /// </summary>
    /// <param name="page">Page number (1-indexed from client, converted to 0-indexed for response)</param>
    /// <param name="pageSize">Page size</param>
    public static PagedResult<T> CreateEmpty(int page = 1, int pageSize = 10)
    {
        var zeroIndexedPage = page - 1;
        var sort = new SortInfo();
        return new PagedResult<T>
        {
            Content = Enumerable.Empty<T>(),
            Pageable = new PageableInfo(zeroIndexedPage, pageSize, sort, (long)zeroIndexedPage * pageSize),
            Last = true,
            TotalElements = 0,
            TotalPages = 0,
            First = zeroIndexedPage == 0,
            Size = pageSize,
            Number = zeroIndexedPage,
            Sort = sort,
            NumberOfElements = 0,
            Empty = true
        };
    }

    /// <summary>
    /// Creates a paged result from items with pagination info.
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="totalElements">Total number of elements across all pages</param>
    /// <param name="page">Current page number (1-indexed from client, converted to 0-indexed for response)</param>
    /// <param name="pageSize">Page size</param>
    public static PagedResult<T> Create(IEnumerable<T> items, long totalElements, int page, int pageSize)
    {
        var itemsList = items.ToList();
        var zeroIndexedPage = page - 1;
        var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalElements / pageSize) : 0;
        var sort = new SortInfo();

        return new PagedResult<T>
        {
            Content = itemsList,
            Pageable = new PageableInfo(zeroIndexedPage, pageSize, sort, (long)zeroIndexedPage * pageSize),
            Last = zeroIndexedPage >= totalPages - 1,
            TotalElements = totalElements,
            TotalPages = totalPages,
            First = zeroIndexedPage == 0,
            Size = pageSize,
            Number = zeroIndexedPage,
            Sort = sort,
            NumberOfElements = itemsList.Count,
            Empty = itemsList.Count == 0
        };
    }
}
