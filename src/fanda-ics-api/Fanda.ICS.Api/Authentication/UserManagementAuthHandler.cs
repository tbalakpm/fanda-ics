using System.Security.Claims;
using System.Text.Encodings.Web;

using Fanda.ICS.Api.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

using Serilog;

namespace Fanda.ICS.Api.Authentication;

public class UserManagementAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly HttpClient _httpClient;
    private readonly AuthSettings _authSettings;

    public UserManagementAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory,
        IOptions<AuthSettings> authSettings)
        : base(options, logger, encoder)
    {
        _httpClient = httpClientFactory.CreateClient("UserManagement");
        _authSettings = authSettings.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return AuthenticateResult.Fail("Authorization header not found");
        }

        string token = authHeader.ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("Invalid token");
        }
        string validApiKey = Environment.GetEnvironmentVariable("API_KEY")!;

        _httpClient.DefaultRequestHeaders.Add("X-API-Key", validApiKey);
        // var response = await _httpClient.GetAsync($"{_authSettings.UserManagementApiUrl}/api/auth/validate?token={token}");
        var response = await _httpClient.PostAsJsonAsync($"{_authSettings.UserManagementApiUrl}/api/auth/validate", new { Token = token });
        Log.Information("Token validation response: {StatusCode}", response.StatusCode);
        if (!response.IsSuccessStatusCode)
        {
            return AuthenticateResult.Fail("Invalid token");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "user"),
            // Add more claims as needed
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}