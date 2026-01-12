namespace Common.Services.ViewModels.RetellAI
{
    public class RetellAIEndpoints
    {
        public string BaseUrl { get; }
        public RetellAIEndpoints(string baseUrl)
        {
            BaseUrl = baseUrl.TrimEnd('/');
        }
        // Agent Endpoints
        public string CreateAgent => $"{BaseUrl}/create-agent";
        public string UpdateAgent(string agentId) => $"{BaseUrl}/update-agent/{agentId}";
        public string DeleteAgent(string agentId) => $"{BaseUrl}/delete-agent/{agentId}";

        // Voice Endpoints
        public string GetVoices => $"{BaseUrl}/list-voices";
        public string GetVoiceById(string voiceId) => $"{BaseUrl}/get-voice/{voiceId}";

        // LLM Endpoints
        public string CreateRetellLLM => $"{BaseUrl}/create-retell-llm";

        // Call Endpoints
        public string CreateWebCall => $"{BaseUrl}/v2/create-web-call";
        public string GetCallById(string callId) => $"{BaseUrl}/v2/get-call/{callId}";


    }
}
