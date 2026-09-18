using System.Security.Claims;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Team3.Backend.Data;
using Team3.Backend.Features.Authentication;
using Team3.Backend.Extensions;
using Team3.Backend.Middleware;

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

// Register application services and Identity.
builder.Services.AddApplicationServices();

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "Team3 Backend API",
            Version = "v1",
            Description = "Backend API for BinX Team 3"
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

app.UseAuthentication();
app.UseMiddleware<UserProvisioningMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();