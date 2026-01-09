using System.Text;
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

        public async Task<TimesheetStatisticsDto> GetTimesheetStatisticsAsync(string userId, DateTime from, DateTime to)
        {
            var url = $"{_baseUrl}/timesheets/statistics?userId={userId}&fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<TimesheetStatisticsDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse?.Data;
        }

        // Helper class to match Spring Boot ApiResponse wrapping
        private class ApiResponse<T>
        {
            public T Data { get; set; }
            public bool Success { get; set; }
            public int Status { get; set; }
            public string Message { get; set; }
            public object Error { get; set; }
        }
    }
}
