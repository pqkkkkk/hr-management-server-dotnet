namespace HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs
{
    /// <summary>
    /// DTO for timesheet statistics from Spring Boot API.
    /// Matches the response from: GET /internal/api/v1/timesheets/statistics/batch
    /// </summary>
    public class TimesheetStatisticsDto
    {
        public string UserId { get; set; } = string.Empty;
        public long TotalDays { get; set; }
        public long MorningPresent { get; set; }
        public long AfternoonPresent { get; set; }
        public long LateDays { get; set; }
        public long TotalLateMinutes { get; set; }
        public long TotalOvertimeMinutes { get; set; }
        public double TotalWorkCredit { get; set; }
    }
}
