using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs;

namespace HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices
{
    public interface ISpringBootApiClient
    {
        Task<List<UserBasicDto>> GetAllUsersAsync(List<string>? roles = null);
        Task<TimesheetStatisticsDto> GetTimesheetStatisticsAsync(string userId, DateTime from, DateTime to);
    }
}
