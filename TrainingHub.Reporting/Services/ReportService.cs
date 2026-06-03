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

        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient("TrainingHubApi");
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("jwt")?.Value;

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<List<EnrollmentStatDto>> GetEnrollmentStatsAsync()
        {
            try
            {
                var client = CreateApiClient();
                var response = await client.GetAsync("api/Reports/enrollments");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<EnrollmentStatDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EnrollmentStatDto>();
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }
            catch (JsonException)
            {
            }

            return new List<EnrollmentStatDto>();
        }

        public async Task<List<InstructorWorkloadDto>> GetInstructorWorkloadAsync()
        {
            try
            {
                var client = CreateApiClient();
                var response = await client.GetAsync("api/Reports/instructors");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<InstructorWorkloadDto>>(jsonString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InstructorWorkloadDto>();
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }
            catch (JsonException)
            {
            }

            return new List<InstructorWorkloadDto>();
        }

        public async Task<List<InstructorWorkloadDto>> GetInstructorsAsync()
        {
            try
            {
                var client = CreateApiClient();
                var response = await client.GetAsync("api/Reports/instructors");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<InstructorWorkloadDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<InstructorWorkloadDto>();
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }
            catch (JsonException)
            {
            }

            return new List<InstructorWorkloadDto>();
        }

        public async Task<List<RevenueReportDto>> GetRevenueAsync()
        {
            try
            {
                var client = CreateApiClient();
                var response = await client.GetAsync("api/reports/revenue");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<RevenueReportDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RevenueReportDto>();
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }
            catch (JsonException)
            {
            }

            return new List<RevenueReportDto>();
        }
    }
}
