namespace Common.Services.ViewModels.ResumeParse
{
    public class MatchMatchingResponse
    {
        public ResumeProfile parseResponse { get; set; } = new ResumeProfile();
        public float matchScore { get; set; } = 0;
        public string recommendation { get; set; } = string.Empty;
    }
}
