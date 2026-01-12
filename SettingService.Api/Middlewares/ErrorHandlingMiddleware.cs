using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;

using System.Net;
using System.Text;
using System.Text.Json;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IErrorLogRepository repo)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var requestData = await GetRequestDataAsync(context);

            var userId = context.User?.Claims?.FirstOrDefault(c => c.Type == "user_id")?.Value;
            var orgId = context.User?.Claims?.FirstOrDefault(c => c.Type == "organization_id")?.Value;

            var log = new ErrorLogModel
            {
                Name = $"{context.Request.Method} {context.Request.Path}",
                Message = ex.Message,
                Detail = ex.ToString(),
                User_Id = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                Organization_Id = string.IsNullOrEmpty(orgId) ? null : Guid.Parse(orgId),
                Created_By = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                Request_Data = requestData
            };

            await repo.LogAsync(log);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Success = false,
                Message = "Something went wrong!",
                Data = ""
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }

    private async Task<string> GetRequestDataAsync(HttpContext context)
    {
        // 1️⃣ Check if it has body (POST/PUT/PATCH)
        if (context.Request.Method == HttpMethods.Post
         || context.Request.Method == HttpMethods.Put
         || context.Request.Method == HttpMethods.Patch)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(body))
                return body;
        }

        // 2️⃣ If no body → fallback to query & route parameters
        var queryParams = context.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        var routeParams = context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString());

        var allParams = new
        {
            Query = queryParams,
            Route = routeParams
        };

        return JsonSerializer.Serialize(allParams);
    }
}
