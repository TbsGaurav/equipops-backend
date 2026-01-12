using Common.Services.JwtAuthentication;
using Common.Services.ViewModels;
using Common.Services.ViewModels.RetellAI;

using InterviewService.Api.Helpers.EncryptionHelpers.Models;
using InterviewService.Api.Middlewares;
using InterviewService.Api.ViewModels.Request.Interview_Que;

using InterViewService.Api;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

using System.Text;

var builder = WebApplication.CreateBuilder(args);
NpgsqlConnection.GlobalTypeMapper.MapComposite<InterviewQuestionBulkDto>("interviews.interview_question_bulk");
// ------------------------------------
// Controllers + Swagger
// ------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------
// Logging + Http Context
// ------------------------------------
builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

// ------------------------------------
// Register Interview Repositories & Services
// ------------------------------------
builder.Services.AddHttpClient();
builder.Services.WithRegisterServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5174", "http://160.187.80.158:7000", "https://hire.intellihrm.in", "http://192.168.1.181:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddHttpClient();
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
builder.Services.Configure<RetellAISettings>(builder.Configuration.GetSection("RetellAI"));

builder.Services.AddSingleton<RetellAIEndpoints>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RetellAISettings>>().Value;
    return new RetellAIEndpoints(settings.BaseUrl);
});

builder.Services.Configure<EncryptionSecretKey>(
    builder.Configuration.GetSection("EncryptionSecretKey")
);

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/Uploads"
});

// ------------------------------------
// Swagger UI
// ------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();