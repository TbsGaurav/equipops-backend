using AuthService.Api;
using AuthService.Api.Middlewares;

using Common.Services.Helper;
using Common.Services.JwtAuthentication;
using Common.Services.ViewModels;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// CORS
// -------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174", "http://160.187.80.158:7000", "https://hire.intellihrm.in")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// -------------------------------
// Controllers + Swagger
// -------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------------
// Logging + Http Context
// -------------------------------
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

// -------------------------------
// Register Services + Repositories
// -------------------------------
builder.Services.WithRegisterServices();

// -------------------------------
// JWT Authentication
// -------------------------------
builder.Services.AddSingleton<JwtTokenHelper>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    throw new Exception("JwtSettings.SecretKey is missing from configuration!");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// -------------------------------
// BUILD APP
// -------------------------------
var app = builder.Build();

// -------------------------------
// Correct Middleware Order
// -------------------------------
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();// JWT
app.UseAuthorization();    // Role/Auth logic
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();
