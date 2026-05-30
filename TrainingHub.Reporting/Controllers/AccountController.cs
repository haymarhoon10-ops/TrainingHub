using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
// We need this using statement to slice open the JWT
using System.IdentityModel.Tokens.Jwt;
using TrainingHub.Reporting.Models;

namespace TrainingHub.Reporting.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("TrainingHubApi");
            var loginData = new { email = model.Email, password = model.Password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonString);
                var token = jsonDoc.RootElement.GetProperty("accessToken").GetString();

                // 1. Slice open the JWT to read the data inside
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // 2. Automatically copy EVERY claim (including the Role!) from the JWT
                var claims = new List<Claim>(jwtToken.Claims);

                // 3. Keep our raw string attached so ReportService can send it to the API later
                claims.Add(new Claim("jwt", token ?? string.Empty));

                // 4. Create the Identity, explicitly telling ASP.NET to look here for Roles
                var identity = new ClaimsIdentity(claims, "ReportingCookie",
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("ReportingCookie", principal);

                return LocalRedirect(model.ReturnUrl ?? Url.Action("Index", "Home")!);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ReportingCookie");
            return RedirectToAction("Index", "Home");
        }
    }
}