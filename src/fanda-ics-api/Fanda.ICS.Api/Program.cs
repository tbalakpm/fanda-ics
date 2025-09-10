using Fanda.ICS.Api;
using Fanda.ICS.Api.Authentication;
using Fanda.ICS.Api.Models;

using Microsoft.AspNetCore.Authentication;

using Scalar.AspNetCore;

using Serilog;

// Log.Logger = new LoggerConfiguration()
//     .WriteTo.Console() // Example sink: write to console
//     .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Example sink: write to file
//     .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration) // Read from appsettings.json
        .CreateLogger();
    builder.Host.UseSerilog();

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddDbContext(builder);
    builder.Services.AddServices();
    // builder.Services.AddJwtAuthentication(configuration);
    builder.Services.Configure<AuthSettings>(
        builder.Configuration.GetSection("AuthSettings"));

    builder.Services.AddHttpClient();

    builder.Services.AddAuthentication("UserManagement")
        .AddScheme<AuthenticationSchemeOptions, UserManagementAuthHandler>(
            "UserManagement", opts => { });
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddJsonOptions();
    builder.Services.AddEndpointsApiExplorer();
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

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapGet("/", () => "Hello from root of Fanda ICS API")
        .WithSummary("Root endpoint")
        .WithName("Root");
    app.MapGet("/api", () => "Hello from api root of Fanda ICS API")
        .RequireAuthorization()
        .WithSummary("API root endpoint")
        .WithName("ApiRoot");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
