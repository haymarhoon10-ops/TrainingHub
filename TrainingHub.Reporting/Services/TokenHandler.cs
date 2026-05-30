using System.Net.Http.Headers;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TrainingHub.Reporting.Services
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null && httpContext.User.Identity?.IsAuthenticated == true)
            {
                // JWT is stored in a claim named "jwt" by the Reporting login flow
                var token = httpContext.User.Claims.FirstOrDefault(c => c.Type == "jwt")?.Value
                            ?? httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Authentication)?.Value;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
