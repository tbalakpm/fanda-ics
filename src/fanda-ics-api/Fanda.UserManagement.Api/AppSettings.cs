namespace Fanda.UserManagement.Api;

public class JwtSettings
{
    //public string SecretKey { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryInMinutes { get; set; } = 60;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = default!;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = default!;
    public string SmtpPassword { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;
}

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
    public string[] AllowedMethods { get; set; } = [];
    public string[] AllowedHeaders { get; set; } = [];
}