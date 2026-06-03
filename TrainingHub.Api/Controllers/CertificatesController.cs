using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Api.DTOs;
using TrainingHub.Data;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class CertificatesController : ControllerBase
    {
        private static readonly Regex CertificateReferencePattern = new("^[A-Za-z0-9-]+$", RegexOptions.Compiled);
        private readonly TrainingHubDbContext _context;

        public CertificatesController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: api/Certificates
        [HttpGet]
        [Authorize(Roles = "TrainingCoordinator")]
        public async Task<IActionResult> Get()
        {
            var list = await _context.Certificates
                .Include(c => c.Trainee)
                .Include(c => c.CertificationTrack)
                .Select(c => new CertificateResponse
                {
                    CertificateId = c.Id,
                    ReferenceNumber = c.CertificateReferenceNumber,
                    TraineeName = c.Trainee != null ? c.Trainee.FullName : string.Empty,
                    TrackName = c.CertificationTrack != null ? c.CertificationTrack.Name : string.Empty,
                    IssueDate = c.IssuedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Certificates/Lookup?traineeId=1&reference=ABC
        [HttpGet("Lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> Lookup([FromQuery] int traineeId, [FromQuery] string? reference)
        {
            if (traineeId <= 0)
                return BadRequest(new ErrorResponse { Message = "Enter a valid trainee ID." });

            reference = reference?.Trim();
            if (string.IsNullOrWhiteSpace(reference))
                return BadRequest(new ErrorResponse { Message = "Certificate reference number is required." });

            if (reference.Length > 100 || !CertificateReferencePattern.IsMatch(reference))
                return BadRequest(new ErrorResponse { Message = "Certificate reference number format is invalid." });

            var cert = await _context.Certificates
                .AsNoTracking()
                .Include(c => c.Trainee)
                .Include(c => c.CertificationTrack)
                .FirstOrDefaultAsync(c => c.TraineeId == traineeId && c.CertificateReferenceNumber == reference);

            if (cert == null)
                return NotFound();

            var completedCourses = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.TraineeId == traineeId
                            && e.Status == "Completed"
                            && e.ResultStatus == "Pass"
                            && e.CourseSession != null
                            && e.CourseSession.Course != null
                            && e.CourseSession.Course.CertificationTrackCourses
                                .Any(ctc => ctc.CertificationTrackId == cert.CertificationTrackId))
                .Select(e => e.CourseSession!.Course!.Title)
                .Distinct()
                .OrderBy(title => title)
                .ToListAsync();

            var result = new CertificateLookupResponse
            {
                CertificateReferenceNumber = cert.CertificateReferenceNumber,
                Status = cert.Status,
                TraineeName = cert.Trainee?.FullName ?? string.Empty,
                CertificationTrack = cert.CertificationTrack?.Name ?? string.Empty,
                TrackDescription = cert.CertificationTrack?.Description ?? string.Empty,
                IssueDate = cert.IssuedAt.ToString("yyyy-MM-dd"),
                CompletedCourses = completedCourses
            };

            return Ok(result);
        }
    }
}
