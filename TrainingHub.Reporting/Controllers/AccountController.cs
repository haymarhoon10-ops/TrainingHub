using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace TrainingHub.Reporting.Models
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient("ApiClient");

            // Package the credentials into JSON to send to the API
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            // Make the request to your API's login endpoint
            var response = await client.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                // Read the token from the response
                var responseString = await response.Content.ReadAsStringAsync();

                using var jsonDoc = JsonDocument.Parse(responseString);
                var token = jsonDoc.RootElement.GetProperty("accessToken").GetString();

                // Create local claims (who the user is to this specific app)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim("jwt", token)
                };

                var identity = new ClaimsIdentity(claims, "ReportingCookie");
                var principal = new ClaimsPrincipal(identity);

                // Sign the user into the MVC app with the cookie
                await HttpContext.SignInAsync("ReportingCookie", principal);

                return RedirectToAction("Index", "Home");
            }

            // If the API says no, show an error
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ReportingCookie");
            return RedirectToAction("Login");
        }
    }
}
