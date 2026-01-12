namespace InterviewService.Api.ViewModels.Response.Interviewer
{
    public class RetellAiVoiceModel
    {
        public string Voice_id { get; set; } = string.Empty;
        public string Voice_name { get; set; } = string.Empty;
        public string Voice_type { get; set; } = string.Empty;
        public string Standard_voice_type { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Accent { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
        public string Avatar_url { get; set; } = string.Empty;
        public string Preview_audio_url { get; set; } = string.Empty;
        public bool Recommended { get; set; }
    }
}
