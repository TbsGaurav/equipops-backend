using Common.Services.ViewModels;
using Common.Services.ViewModels.RetellAI;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

using OrganizationService.Api;
using OrganizationService.Api.Helpers.EncryptionHelpers.Models;
using OrganizationService.Api.Middlewares;
using OrganizationService.Api.ViewModels.Request.Organzation;
using OrganizationService.Api.ViewModels.Request.User;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

NpgsqlConnection.GlobalTypeMapper.MapComposite<UserAccessRoleBulkDto>("master.user_access_role_bulk");
NpgsqlConnection.GlobalTypeMapper.MapComposite<OrganizationCreateUpdateRequestViewModel.LocationCreateUpdateItemViewModel>("master.organization_location_bulk");
NpgsqlConnection.GlobalTypeMapper.MapComposite<OrganizationCreateUpdateRequestViewModel.DepartmentsCreateUpdateItemViewModel>("master.industry_type_department_bulk");

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
// Register Organization Repositories & Services
// ------------------------------------
builder.Services.AddHttpClient();
builder.Services.WithRegisterServices();

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

builder.Services.AddHttpClient();

builder.Services.Configure<EncryptionSecretKey>(
    builder.Configuration.GetSection("EncryptionSecretKey")
);
builder.Services.Configure<RetellAISettings>(builder.Configuration.GetSection("RetellAI"));

builder.Services.AddSingleton<RetellAIEndpoints>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RetellAISettings>>().Value;
    return new RetellAIEndpoints(settings.BaseUrl);
});


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
app.UseAuthorization();
//app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();