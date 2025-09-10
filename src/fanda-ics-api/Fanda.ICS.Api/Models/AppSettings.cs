using System;

namespace Fanda.ICS.Api.Models;

public class AuthSettings
{
    public string UserManagementApiUrl { get; set; } = string.Empty;
}

public class JwtSettings
{
    //public string SecretKey { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryInMinutes { get; set; } = 60;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
    public string[] AllowedMethods { get; set; } = [];
    public string[] AllowedHeaders { get; set; } = [];
}
