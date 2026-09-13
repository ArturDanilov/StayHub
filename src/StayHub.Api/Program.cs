using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using StayHub.Business.Interfaces;
using StayHub.Business.Managers;
using StayHub.Dal.Data;
using StayHub.Dal.Repositories;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StayHub.Api.Authentication;
using StayHub.Api.Common;
using StayHub.Api.Health;
using StayHub.Api.Seed;
using StayHub.Api.Integration;
using StayHub.Api.Synchronization;
using StayHub.Api.Assistant;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("StayHubDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'StayHubDb' is missing. Configure ConnectionStrings__StayHubDb.");
}

builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the JWT access token."
        });

    options.AddSecurityRequirement(
        document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});
builder.Services.AddDbContext<StayHubDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyManager, PropertyManager>();

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationManager, ReservationManager>();
builder.Services.AddScoped<IAssistantManager, AssistantManager>();
builder.Services
    .AddOptions<AiAssistantOptions>()
    .Bind(builder.Configuration.GetSection(AiAssistantOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out _),
        "AiAssistant BaseAddress must be an absolute URL.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "AiAssistant Model is required.")
    .Validate(options => options.TimeoutSeconds > 0, "AiAssistant TimeoutSeconds must be positive.")
    .ValidateOnStart();
builder.Services.AddHttpClient<IAssistantClient, OllamaAssistantClient>((services, client) =>
{
    var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiAssistantOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseAddress.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddScoped<ISourceRepository, SourceRepository>();
builder.Services.AddScoped<ISourceManager, SourceManager>();

builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IGuestManager, GuestManager>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthenticationManager, AuthenticationManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ISynchronizationRunRepository, SynchronizationRunRepository>();
builder.Services.AddSingleton<ISynchronizationExecutionGate, SynchronizationExecutionGate>();
builder.Services.AddScoped<ISynchronizationManager, SynchronizationManager>();
builder.Services.AddScoped<AutomaticSynchronizationJob>();
builder.Services
    .AddOptions<AutomaticSynchronizationOptions>()
    .Bind(builder.Configuration.GetSection(AutomaticSynchronizationOptions.SectionName))
    .Validate(options => options.IntervalMinutes > 0, "IntervalMinutes must be greater than zero.")
    .Validate(options => options.InitialDelaySeconds >= 0, "InitialDelaySeconds cannot be negative.")
    .ValidateOnStart();
builder.Services.AddHostedService<ReservationSynchronizationWorker>();
builder.Services.AddHttpClient<IExternalReservationClient, ExternalReservationClient>(client =>
{
    // A scale-to-zero PMS container can need more than 15 seconds for its first cold start.
    client.Timeout = TimeSpan.FromSeconds(60);
});

var jwtOptions = builder.Configuration
                     .GetSection(JwtOptions.SectionName)
                     .Get<JwtOptions>()
                 ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer)
    || string.IsNullOrWhiteSpace(jwtOptions.Audience)
    || jwtOptions.Key.Length < 32
    || jwtOptions.ExpirationMinutes <= 0)
{
    throw new InvalidOperationException(
        "JWT configuration is invalid. Issuer, Audience, a key of at least 32 characters, " +
        "and a positive ExpirationMinutes value are required.");
}

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.ReadAccess,
        policy => policy.RequireRole(
            AppRoles.Admin,
            AppRoles.Receptionist,
            AppRoles.Viewer));

    options.AddPolicy(
        AuthorizationPolicies.ManageReservations,
        policy => policy.RequireRole(
            AppRoles.Admin,
            AppRoles.Receptionist));

    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole(AppRoles.Admin));
});
var app = builder.Build();

if (builder.Configuration.GetValue("DatabaseInitialization:ApplyMigrations", true))
{
    await DatabaseSeeder.SeedAsync(app.Services);
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready")
    });

app.Run();
