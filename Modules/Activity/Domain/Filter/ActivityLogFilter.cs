namespace HrManagement.Api.Modules.Activity.Domain.Filter;

/// <summary>
/// Filter for querying activity logs with pagination
/// </summary>
public record ActivityLogFilter
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Filter by activity ID
    /// </summary>
    public string? ActivityId { get; init; }

    /// <summary>
    /// Filter by employee ID
    /// </summary>
    public string? EmployeeId { get; init; }

    /// <summary>
    /// Filter by log status
    /// </summary>
    public Entities.ActivityEnums.ActivityLogStatus? Status { get; init; }

    /// <summary>
    /// Filter by log date (from)
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Filter by log date (to)
    /// </summary>
    public DateTime? ToDate { get; init; }
}
