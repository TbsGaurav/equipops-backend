using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Webhook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InterviewService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _iWebhookService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
        {
            _iWebhookService = webhookService;
            _logger = logger;
        }

        [HttpPost("retell/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonElement body)
        {
            var eventName = body.GetProperty("event").GetString();

            // Nested object: call
            var call = body.GetProperty("call");

            var callId = call.GetProperty("call_id").GetString();
            var callStatus = call.GetProperty("call_status").GetString();


            _logger.LogInformation(
                  "Retell Webhook | Event={Event} | CallId={CallId} | Status={Status}",
                  eventName,
                  callId,
                  callStatus
              );

            // Pass full body or extracted values to service
            await _iWebhookService.HandleRetellEventAsync(eventName, callId, body);

            return Ok();
        }
    }
}
