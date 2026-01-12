using Newtonsoft.Json;

namespace InterviewService.Api.ViewModels.Response.Interview_transcript
{
    public class RetellGetCallResponse
    {
        [JsonProperty("call_id")]
        public string Call_id { get; set; } = string.Empty;

        [JsonProperty("call_status")]
        public string CallStatus { get; set; } = string.Empty;

        [JsonProperty("transcript")]
        public string Transcript { get; set; } = string.Empty;

        [JsonProperty("transcript_object")]
        public List<RetellTranscriptItem> TranscriptObject { get; set; } = new List<RetellTranscriptItem>();
    }
    public class RetellTranscriptItem
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }

}
