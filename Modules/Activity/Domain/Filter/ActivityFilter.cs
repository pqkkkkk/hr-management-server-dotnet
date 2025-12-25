namespace HrManagement.Api.Modules.Activity.Domain.Filter;

/// <summary>
/// Filter for querying activities
/// </summary>
public record ActivityFilter
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Filter by activity name (contains)
    /// </summary>
    public string? NameContains { get; init; }

    /// <summary>
    /// Filter by activity type
    /// </summary>
    public ActivityEnums.ActivityType? Type { get; init; }

    /// <summary>
    /// Filter by status
    /// </summary>
    public ActivityEnums.ActivityStatus? Status { get; init; }

    /// <summary>
    /// Filter activities that are currently active (OPEN or IN_PROGRESS)
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filter by start date (activities starting from this date)
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Filter by end date (activities ending before this date)
    /// </summary>
    public DateTime? ToDate { get; init; }
}
