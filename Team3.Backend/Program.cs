using System.Security.Claims;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Team3.Backend.Data;
using Team3.Backend.Features.Authentication;
using Team3.Backend.Extensions;
using Team3.Backend.Middleware;
using Team3.Backend.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

// Initialize Firebase Admin SDK.
FirebaseAuthenticationService.Initialize();

var firebaseProjectId = builder.Configuration["Firebase:ProjectId"]
    ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")
    ?? FirebaseApp.DefaultInstance?.Options.ProjectId;

if (string.IsNullOrWhiteSpace(firebaseProjectId))
{
    throw new InvalidOperationException(
        "Firebase project ID is not configured. Set Firebase:ProjectId " +
        "or FIREBASE_PROJECT_ID.");
}

var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':', 2);

    connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Database = databaseUri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    }.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var corsOrigins = builder.Configuration
    .GetSection("CorsOrigins")
    .GetChildren()
    .Select(section => section.Value)
    .OfType<string>()
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .ToArray();

if (corsOrigins.Length == 0)
{
    corsOrigins = ["http://localhost:5173"];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority =
            $"https://securetoken.google.com/{firebaseProjectId}";
        options.Audience = firebaseProjectId;
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer =
                $"https://securetoken.google.com/{firebaseProjectId}",
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis")
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION")
        ?? "localhost:6379";

    options.InstanceName = "Riwaq:";
});

builder.Services.AddSignalR();
// Register application services and Identity.
builder.Services.AddApplicationServices();

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlDocumentationFile =
        Path.Combine(AppContext.BaseDirectory, "Team3.Backend.xml");

    options.IncludeXmlComments(xmlDocumentationFile);

    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "Team3 Backend API",
            Version = "v1",
            Description = "Backend API for BinX Team 3\n\n" +
                "Realtime notifications: connect to /hubs/notifications " +
                "and listen for NotificationReceived. The payload is " +
                "NotificationResponse."
        }
    );

    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Firebase ID token"
        }
    );

    options.OperationFilter<AuthorizeOperationFilter>();

    options.TagActionsBy(apiDescription =>
    {
        var controller = apiDescription.ActionDescriptor.RouteValues["controller"];

        return [controller switch
        {
            "Authentication" => "Authentication",
            "Users" => "Users",
            "Skills" => "Skills",
            "Interests" => "Interests",
            "LearningDirections" => "Learning Directions",
            "Experiences" => "Experiences",
            "Progress" => "Progress",
            "Points" => "Points",
            "Notifications" => "Notifications",
            "LearningSessions" => "Learning Sessions",
            "EducationalContent" => "Educational Content",
            "Comments" => "Comments",
            "ConnectionRequests" => "Connection Requests",
            "Recommendations" => "Recommendations",
            _ => controller ?? "Other"
        }];
    });
});

var app = builder.Build();

// Swagger UI
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Team3 Backend API v1"
    );

    options.RoutePrefix = "swagger";
});

app.UseCors("Frontend");
app.UseAuthentication();
app.UseMiddleware<UserProvisioningMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<Team3.Backend.Features.Notifications.NotificationsHub>(
    "/hubs/notifications");

app.Run();