using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainingHub.Reporting.Services;

namespace TrainingHub.Reporting.Controllers
{
    // Ensure only logged-in users can access any of these reports
    [Authorize(Roles = "TrainingCoordinator")]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;

        // Dependency Injection: ASP.NET automatically hands us the ReportService here
        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var enrollmentStats = await _reportService.GetEnrollmentStatsAsync();

            return View(enrollmentStats);
        }

        [HttpGet]
        public async Task<IActionResult> InstructorWorkload()
        {
            var workloadData = await _reportService.GetInstructorWorkloadAsync();
            return View(workloadData);
        }
        [HttpGet]
        public async Task<IActionResult> Revenue()
        {
            var data = await _reportService.GetRevenueAsync();
            return View(data);
        }
    }
}