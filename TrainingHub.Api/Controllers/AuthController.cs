using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TrainingHub.Models;
using TrainingHub.Security;

namespace TrainingHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtOptions _jwtOptions;

        public AuthController(UserManager<ApplicationUser> userManager, IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(_jwtOptions.DurationInMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? request.Email),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? request.Email),
                new(ClaimTypes.Email, user.Email ?? request.Email),
                new(ClaimTypes.GivenName, user.FullName)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
                    SecurityAlgorithms.HmacSha256));

            return Ok(new AuthResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt,
                user.Email ?? request.Email,
                user.FullName,
                roles.ToArray()));
        }

        [HttpGet("me")]
        public ActionResult<AuthMeResponse> Me()
        {
            var roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();

            return Ok(new AuthMeResponse(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                roles));
        }
    }

    public sealed record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public sealed record AuthResponse(
        string AccessToken,
        DateTime ExpiresAtUtc,
        string Email,
        string FullName,
        string[] Roles);

    public sealed record AuthMeResponse(
        string UserId,
        string Email,
        string FullName,
        string[] Roles);
}