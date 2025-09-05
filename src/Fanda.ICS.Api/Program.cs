using System.Text;

using Fanda.ICS.Api;
using Fanda.ICS.Api.Data;
using Fanda.ICS.Api.Endpoints;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

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

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    // Add services to the container.
    string connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
    builder.Services.AddDbContextPool<AppDbContext>(options =>
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNodaTime(); // Enable NodaTime integration
        var dataSource = dataSourceBuilder.Build();

        options.UseNpgsql(dataSource)
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .UseSnakeCaseNamingConvention();
    });

    builder.Services.AddServices();
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    })
        .AddJwtBearer(options =>
        {
            options.Authority = configuration["Jwt:Authority"];
            options.Audience = configuration["Jwt:Audience"];
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidIssuer = configuration["Jwt:Issuer"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:secret"]!))
            };
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
    app.UseAuthentication();
    app.MapGet("/", () => "Hello from Fanda ICS API")
        .WithName("HelloWorld");
    app.MapUserEndpoints();

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
