using System.Text.Json;
using HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices.DTOs;
using Microsoft.Extensions.Configuration;

namespace HrManagement.Api.Modules.Reward.Infrastructure.ExternalServices
{
    public class SpringBootApiClient : ISpringBootApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SpringBootApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ExternalServices:SpringBootApi:BaseUrl"] ?? "http://localhost:8080/internal/api/v1";
        }

        public async Task<List<UserBasicDto>> GetAllUsersAsync(List<string>? roles = null)
        {
            var url = $"{_baseUrl}/profiles";
            if (roles != null && roles.Any())
            {
                var roleParams = string.Join(",", roles);
                url += $"?roles={roleParams}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserBasicDto>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse?.Data ?? new List<UserBasicDto>();
        }

        public async Task<List<TimesheetStatisticsDto>> GetBatchTimesheetStatisticsAsync(
            List<string> userIds, DateTime startDate, DateTime endDate)
        {
            // Build URL with repeated userIds query params: ?userIds=u1&userIds=u2&...
            var userIdParams = string.Join("&", userIds.Select(id => $"userIds={Uri.EscapeDataString(id)}"));
            var url = $"{_baseUrl}/timesheets/statistics/batch?{userIdParams}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<TimesheetStatisticsDto>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse?.Data ?? new List<TimesheetStatisticsDto>();
        }

        // Helper class to match Spring Boot ApiResponse wrapping
        private class ApiResponse<T>
        {
            public T Data { get; set; } = default!;
            public bool Success { get; set; }
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
            public object? Error { get; set; }
        }
    }
}

