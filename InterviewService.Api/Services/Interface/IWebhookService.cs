using InterviewService.Api.ViewModels.Request.Webhook;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InterviewService.Api.Services.Interface
{
    public interface IWebhookService
    {
        Task<IActionResult> HandleRetellEventAsync(string eventName,string callId, JsonElement body);
    }
}
