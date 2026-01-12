using Common.Services.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace Common.Services.JwtAuthentication
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secret;
        private const string InternalApiKey = "SERVICE_KEY_123"; // your internal key
        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings)
        {
            _next = next;
            _secret = jwtSettings.Value.SecretKey;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(path) && (path.Contains("/interview/uploads") || (path.Contains("/webhook/retell/webhook"))
                 || (path.Contains("/org/uploads")
                || path.Contains("/auth/login")
                || path.Contains("/auth/forgot-password")
                || path.Contains("/auth/reset-password")
                || path.Contains("/organization/verify-email")
                || path.Contains("/interviewdetail/get_interview_detail")
                || path.Contains("/organization/create-update")
                || path.Contains("/interview/call-register")
                || path.Contains("/interview/verify-token")
                || path.Contains("/interview/create_token")
               || path.Contains("/interview/end-call")
               || path.Contains("/organization/get-industry-deparment")
               || path.Contains("/candidate/create-update")
               || path.Contains("/masterdropdown/list")
               || path.Contains("/organizationlocation/get-county-list")
               || path.Contains("/organizationlocation/get-states-by-country")
               || path.Contains("/organizationlocation/get-cities-by_state")
               || path.Contains("/subscription/getbyorganizationid")
               || path.Contains("/interviewform/interview_formbyinterviewformid")
               )))
            {
                await _next(context);
                return;
            }

            // ✅ 2. Check for internal API key first
            if (context.Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
            {
                if (apiKey == InternalApiKey)
                {
                    // Internal call allowed, skip JWT validation
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Missing Authorization header." }));
                return;
            }


            var token = authHeader.ToString()
                .Replace("Bearer", "", StringComparison.OrdinalIgnoreCase)
                .Replace(":", "")
                .Trim();
            if (!token.Contains(".") || token.Split('.').Length != 3)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message = "Malformed token received."
                }));
                return;
            }

            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
                var organizationId = jwtToken.Claims.FirstOrDefault(c => c.Type == "organization_id")?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        message = "Invalid token: missing user id."
                    }));
                    return;
                }

                // ✅ role is required
                if (string.IsNullOrWhiteSpace(role))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        message = "Invalid token: missing role."
                    }));
                    return;
                }

                // ✅ organization required ONLY for non–Super Admin
                if (!role.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(organizationId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        message = "Invalid token: missing organization."
                    }));
                    return;
                }


                // Attach info to HttpContext
                context.Items["UserId"] = userId;
                context.Items["OrganizationId"] = organizationId; // null for Super Admin
                context.Items["Email"] = email;
                context.Items["Role"] = role;

                context.Request.Headers["X-UserId"] = userId;
                context.Request.Headers["X-Email"] = email;
                context.Request.Headers["X-Role"] = role;
                context.Request.Headers["X-Access-Token"] = token;
            }
            catch (SecurityTokenExpiredException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Token expired." }));
                return;
            }
            catch (SecurityTokenException)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Invalid token." }));
                return;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = $"Unauthorized: {ex.Message}" }));
                return;
            }

            await _next(context);
        }
    }
}
