using Newtonsoft.Json;

namespace InterviewService.Api.ViewModels.Request.Interview
{
    public class CallRegisterRequestViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
    public class RetellCallRequest
    {
        [JsonProperty("agent_id")]
        public string AgentId { get; set; } = default!;

        [JsonProperty("retell_llm_dynamic_variables")]
        public Dictionary<string, string> DynamicVariables { get; set; } = new();

        [JsonProperty("agent_override")]
        public AgentOverride AgentOverride { get; set; } = new();
    }

    public class AgentOverride
    {
        [JsonProperty("agent")]
        public Agent Agent { get; set; } = new();
    }

    public class Agent
    {
        [JsonProperty("language")]
        public string Language { get; set; } = "en-US";
    }
}
