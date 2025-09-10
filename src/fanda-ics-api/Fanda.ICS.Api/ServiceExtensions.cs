using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Fanda.ICS.Api.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

namespace Fanda.ICS.Api;

public static class ServiceExtensions
{

    public static void AddServices(this IServiceCollection services)
    {
        //services.AddScoped<IUserService, UserService>();
    }

    public static void AddDbContext(this IServiceCollection services, WebApplicationBuilder builder)
    {
        string connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")!;
        services.AddDbContextPool<AppDbContext>(options =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseNodaTime(); // Enable NodaTime integration
            var dataSource = dataSourceBuilder.Build();

            options.UseNpgsql(dataSource)
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
                .UseSnakeCaseNamingConvention();
        });
    }

    public static void AddJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            // Configure System.Text.Json options here
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = true;
            // options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            // Add custom converters
            // options.SerializerOptions.Converters.Add(new CustomDateTimeConverter());
        });

        // services.AddControllers()
        //     .AddJsonOptions(options =>
        //     {
        //         options.JsonSerializerOptions.PropertyNamingPolicy = null;
        //         options.JsonSerializerOptions.DictionaryKeyPolicy = null;
        //     });
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
        services.AddAuthentication(options =>
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

        // var jwtSettings = configuration.GetSection("JwtSettings");
        // string secretKey = jwtSettings.GetValue<string>("SecretKey")!;

        // services.AddAuthentication(options =>
        // {
        //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // })
        // .AddJwtBearer(options =>
        // {
        //     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        //     {
        //         ValidateIssuer = true,
        //         ValidateAudience = true,
        //         ValidateLifetime = true,
        //         ValidateIssuerSigningKey = true,
        //         ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        //         ValidAudience = jwtSettings.GetValue<string>("Audience"),
        //         IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey))
        //     };
        // });
    }
}
