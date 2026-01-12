using Newtonsoft.Json;
using System.Text.Json;

namespace InterviewService.Api.ViewModels.Request.Webhook
{
    public class WebhookRequestViewModel
    {
        [JsonProperty("event")]
        public string Event { get; set; } = string.Empty;

        [JsonProperty("callId")]
        public string callId { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [JsonProperty("data")]
        public JsonElement? Data { get; set; }
    }
}
