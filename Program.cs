using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using store.Common;
using store.Data;
using store.Middlewares;
using store.Models;
using store.Services.Cart;
using store.Services.Categories;
using store.Services.Orders;
using store.Services.Products;
using System.Linq;
using System.Text;

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
// Controllers + Consistent Validation Response
//////////////////////////////////////////////////////////

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value is not null && x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(
                ApiResponse.FailResponse("Validation failed.", errors)
            );
        };
    });

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

var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.");

var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing from configuration.");

var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing from configuration.");

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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

//////////////////////////////////////////////////////////
// Application Services
//////////////////////////////////////////////////////////

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();

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
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("api", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 100;
        config.QueueLimit = 0;
    });
});

//////////////////////////////////////////////////////////
// Swagger
//////////////////////////////////////////////////////////

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "store API",
        Version = "v1",
        Description = "Professional e-commerce backend built with ASP.NET Core Web API."
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token only.",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

//////////////////////////////////////////////////////////
// Middleware
//////////////////////////////////////////////////////////

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

//////////////////////////////////////////////////////////
// Endpoints
//////////////////////////////////////////////////////////

app.MapControllers().RequireRateLimiting("api");
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
