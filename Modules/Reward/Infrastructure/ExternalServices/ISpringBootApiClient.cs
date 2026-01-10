using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs;

namespace HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices
{
    public interface ISpringBootApiClient
    {
        Task<List<UserBasicDto>> GetAllUsersAsync(List<string>? roles = null);
        
        /// <summary>
        /// Get batch timesheet statistics for multiple users in a single call.
        /// </summary>
        /// <param name="userIds">List of user IDs (max 100)</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>List of statistics for each user</returns>
        Task<List<TimesheetStatisticsDto>> GetBatchTimesheetStatisticsAsync(
            List<string> userIds, DateTime startDate, DateTime endDate);
    }
}
