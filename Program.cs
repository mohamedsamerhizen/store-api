using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using store.Data;
using store.Models;
using store.Services.Orders;
using store.Services.Products;
using store.Middlewares;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//////////////////////////////////////////////////////////
// Serilog Logging
//////////////////////////////////////////////////////////

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

//////////////////////////////////////////////////////////
// Controllers
//////////////////////////////////////////////////////////

builder.Services.AddControllers();

//////////////////////////////////////////////////////////
// Response Caching
//////////////////////////////////////////////////////////

builder.Services.AddResponseCaching();

//////////////////////////////////////////////////////////
// FluentValidation
//////////////////////////////////////////////////////////

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//////////////////////////////////////////////////////////
// DbContext
//////////////////////////////////////////////////////////

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//////////////////////////////////////////////////////////
// Identity
//////////////////////////////////////////////////////////

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//////////////////////////////////////////////////////////
// JWT Authentication
//////////////////////////////////////////////////////////

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
        options.DefaultScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

//////////////////////////////////////////////////////////
// Application Services
//////////////////////////////////////////////////////////

builder.Services.AddScoped<IOrderService, OrderService>();

//////////////////////////////////////////////////////////
// Memory Cache
//////////////////////////////////////////////////////////

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ProductCacheService>();

//////////////////////////////////////////////////////////
// Health Checks
//////////////////////////////////////////////////////////

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

//////////////////////////////////////////////////////////
// Rate Limiting
//////////////////////////////////////////////////////////

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 100;
        config.QueueLimit = 0;
    });
});

//////////////////////////////////////////////////////////
// Problem Details
//////////////////////////////////////////////////////////

builder.Services.AddProblemDetails();

var app = builder.Build();

//////////////////////////////////////////////////////////
// Middleware
//////////////////////////////////////////////////////////

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseResponseCaching();

app.UseMiddleware<ETagMiddleware>();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//////////////////////////////////////////////////////////
// Health Check Endpoint
//////////////////////////////////////////////////////////

app.MapHealthChecks("/health");

//////////////////////////////////////////////////////////
// Identity Seeder
//////////////////////////////////////////////////////////

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = services.GetRequiredService<IConfiguration>();

    await IdentitySeeder.SeedAsync(services, configuration);
}

//////////////////////////////////////////////////////////
// Run Application
//////////////////////////////////////////////////////////

try
{
    Log.Information("Application starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}