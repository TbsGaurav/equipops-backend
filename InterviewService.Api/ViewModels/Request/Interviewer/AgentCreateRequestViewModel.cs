using Newtonsoft.Json;

namespace InterviewService.Api.ViewModels.Request.Interviewer
{
    public class AgentCreateRequestViewModel
    {
        [JsonProperty("response_engine")]
        public AgentData ResponseEngine { get; set; } = new AgentData();
        [JsonProperty("voice_id")]
        public string VoiceId { get; set; } = string.Empty;
        [JsonProperty("agent_name")]
        public string AgentName { get; set; } = string.Empty;
        [JsonProperty("system_prompt")]
        public string SystemPrompt { get; set; } = string.Empty;
        [JsonProperty("language")]
        public string Language { get; set; } = "en-US";

        [JsonProperty("interruptible")]
        public bool Interruptible { get; set; } = true;

        [JsonProperty("end_call_on_goodbye")]
        public bool EndCallOnGoodbye { get; set; } = true;
    }
    public class AgentData
    {
        [JsonProperty("llm_id")]
        public string LlmId { get; set; } = string.Empty;
        [JsonProperty("type")]
        public string Type { get; set; } = "retell-llm";
    }
}
