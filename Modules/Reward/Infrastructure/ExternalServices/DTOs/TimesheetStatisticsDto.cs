namespace HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs
{
    public class TimesheetStatisticsDto
    {
        public string UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalWorkDays { get; set; }
        public int DaysOnTime { get; set; }
        public int DaysLate { get; set; }
        public int TotalOvertimeMinutes { get; set; }
        public int DaysFullAttendance { get; set; }
    }
}
