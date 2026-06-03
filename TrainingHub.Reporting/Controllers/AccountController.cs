using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Net.Http;
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

            try
            {
                var client = _httpClientFactory.CreateClient("TrainingHubApi");
                var loginData = new { email = model.Email, password = model.Password };
                var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(jsonString);

                    if (!jsonDoc.RootElement.TryGetProperty("accessToken", out var tokenProperty))
                    {
                        ModelState.AddModelError(string.Empty, "Login succeeded but the API response did not contain an access token.");
                        return View(model);
                    }

                    var token = tokenProperty.GetString();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        ModelState.AddModelError(string.Empty, "Login succeeded but the API returned an empty access token.");
                        return View(model);
                    }

                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    var claims = new List<Claim>(jwtToken.Claims)
                    {
                        new Claim("jwt", token)
                    };

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
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "The Reporting application could not reach the API. Check the configured API base URL and ensure the API is running.");
                return View(model);
            }
            catch (TaskCanceledException)
            {
                ModelState.AddModelError(string.Empty, "The login request to the API timed out. Try again in a moment.");
                return View(model);
            }
            catch (JsonException)
            {
                ModelState.AddModelError(string.Empty, "The API returned an unexpected response during login.");
                return View(model);
            }
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
