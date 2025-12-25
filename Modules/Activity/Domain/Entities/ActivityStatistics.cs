namespace HrManagement.Api.Modules.Activity.Domain.Entities;

/// <summary>
/// Activity statistics for admin/manager view
/// </summary>
public class ActivityStatistics
{
    public string ActivityId { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public int ActiveParticipants { get; set; }
    public decimal TotalDistance { get; set; }
    public int TotalLogs { get; set; }
    public int PendingLogs { get; set; }
    public int ApprovedLogs { get; set; }
    public int RejectedLogs { get; set; }
    public decimal AverageDistancePerParticipant { get; set; }
}
