using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using TrainingHub.Data;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CertificatesController : ControllerBase
    {
        private readonly TrainingHubDbContext _context;

        public CertificatesController(TrainingHubDbContext context)
        {
            _context = context;
        }

        // GET: api/Certificates
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _context.Certificates
                .Include(c => c.Trainee)
                .Include(c => c.CertificationTrack)
                .Select(c => new
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
                return BadRequest("Invalid traineeId.");

            if (string.IsNullOrWhiteSpace(reference))
                return BadRequest("Reference is required.");

            var cert = await _context.Certificates
                .Include(c => c.Trainee)
                .Include(c => c.CertificationTrack)
                .FirstOrDefaultAsync(c => c.TraineeId == traineeId && c.CertificateReferenceNumber == reference);

            if (cert == null)
                return NotFound();

            var result = new
            {
                Status = "Valid",
                TraineeName = cert.Trainee?.FullName ?? string.Empty,
                CertificationTrack = cert.CertificationTrack?.Name ?? string.Empty,
                IssueDate = cert.IssuedAt.ToString("yyyy-MM-dd")
            };

            return Ok(result);
        }
    }
}
