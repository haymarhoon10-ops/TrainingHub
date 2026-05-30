using System.Net.Http.Headers;
using System.Text.Json;
using TrainingHub.Reporting.Models;

namespace TrainingHub.Reporting.Services
{
    public class ReportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<EnrollmentStatDto>> GetEnrollmentStatsAsync()
        {
            // 1. Get the pre-configured HTTP client ("ApiClient" from Program.cs)
            var client = _httpClientFactory.CreateClient("TrainingHubApi");

            // 2. Reach into the user's current session to find their JWT "wristband"
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("jwt")?.Value;

            // 3. Attach the token to the request's Authorization header
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("api/Reports/enrollments");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<EnrollmentStatDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EnrollmentStatDto>();
            }

            // If it fails (e.g., unauthorized or server error), return an empty list
            return new List<EnrollmentStatDto>();
        }

        public async Task<List<InstructorWorkloadDto>> GetInstructorWorkloadAsync()
        {
            var client = _httpClientFactory.CreateClient("TrainingHubApi");

            // Use the same claim name the AccountController stores ("jwt")
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("jwt")?.Value;

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Call the specific endpoint for the workload data
            var response = await client.GetAsync("api/Reports/instructors");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<InstructorWorkloadDto>>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InstructorWorkloadDto>();
            }

            return new List<InstructorWorkloadDto>();
        }

        public async Task<List<InstructorWorkloadDto>> GetInstructorsAsync()
        {
            var client = _httpClientFactory.CreateClient("TrainingHubApi");

            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("jwt")?.Value;
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("api/Reports/instructors");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<InstructorWorkloadDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InstructorWorkloadDto>();
        }

        public async Task<IEnumerable<RevenueReportDto>?> GetRevenueAsync()
        {
            var client = _httpClientFactory.CreateClient("TrainingHubApi");

            // Grab the JWT from the context (You already wrote this logic!)
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("api/reports/revenue");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<RevenueReportDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }
    }
}