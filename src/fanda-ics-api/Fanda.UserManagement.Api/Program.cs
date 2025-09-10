using System.Text;

using Fanda.UserManagement.Api.Authentication;
using Fanda.UserManagement.Api.Data;
using Fanda.UserManagement.Api.Endpoints;
using Fanda.UserManagement.Api.Models;
using Fanda.UserManagement.Api.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

using Scalar.AspNetCore;

using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();

    // Database configuration
    string connectionString = Environment.GetEnvironmentVariable("USERDB_CONNECTION")
        ?? throw new InvalidOperationException("USERDB_CONNECTION environment variable is not set");

    builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime();
        var dataSource = dataSourceBuilder.Build();

        options.UseNpgsql(dataSource)
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .UseSnakeCaseNamingConvention();
    });

    // ASP.NET Core Identity configuration
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Configuration binding
    var jwtSettings = new JwtSettings();
    configuration.GetSection("JwtSettings").Bind(jwtSettings);
    builder.Services.AddSingleton(jwtSettings);

    var emailSettings = new EmailSettings();
    configuration.GetSection("EmailSettings").Bind(emailSettings);
    builder.Services.AddSingleton(emailSettings);

    // JWT Authentication
    string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET environment variable is not set");
    string validApiKey = Environment.GetEnvironmentVariable("API_KEY")
        ?? throw new InvalidOperationException("API_KEY environment variable is not set");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"success\":false,\"message\":\"Unauthorized\",\"data\":null,\"errors\":[]}");
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>("ApiKey", opts => { });

    // CORS
    var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>();
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsSettings?.AllowedOrigins ?? [])
                 .WithMethods(corsSettings?.AllowedMethods ?? [])
                 .WithHeaders(corsSettings?.AllowedHeaders ?? [])
                 .AllowCredentials();
        });
    });

    // Authorization
    builder.Services.AddAuthorization();

    // Register application services
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    // Database migration and seeding
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Apply migrations
        await dbContext.Database.MigrateAsync();

        // Seed roles
        foreach (var roleName in ApplicationRole.Names.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    Description = $"{roleName} role",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await roleManager.CreateAsync(role);
                Log.Information("Created role: {RoleName}", roleName);
            }
        }

        // Seed admin user
        string adminEmail = Environment.GetEnvironmentVariable("ADMIN_USER_EMAIL")
            ?? throw new InvalidOperationException("ADMIN_USER_EMAIL environment variable is not set");
        string adminPassword = Environment.GetEnvironmentVariable("ADMIN_USER_PASSWORD")
            ?? throw new InvalidOperationException("ADMIN_USER_PASSWORD environment variable is not set");

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationRole.Names.Admin);
                Log.Information("Created admin user: {AdminEmail}", adminEmail);
            }
            else
            {
                Log.Error("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    // Add this before app.UseAuthorization()
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    // Map endpoints
    app.MapAuthEndpoints();
    app.MapUserEndpoints();

    // Health check endpoints
    app.MapGet("/", () => new
    {
        service = "Fanda User Management API",
        status = "Running",
        version = "2.0.0",
        timestamp = DateTime.UtcNow,
        architecture = "Clean Architecture with ASP.NET Core Identity"
    })
        .WithName("Root")
        .WithSummary("API health check")
        .WithOpenApi();

    app.MapGet("/api", () => new
    {
        service = "Fanda User Management API",
        version = "2.0.0",
        status = "Running",
        timestamp = DateTime.UtcNow,
        endpoints = new
        {
            authentication = "/api/auth/*",
            users = "/api/users/*",
            documentation = "/scalar/v1"
        }
    })
        .WithName("ApiInfo")
        .WithSummary("API information")
        .WithOpenApi();

    Log.Information("🚀 Fanda User Management API starting up...");
    Log.Information("📖 API Documentation available at: /scalar/v1");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
