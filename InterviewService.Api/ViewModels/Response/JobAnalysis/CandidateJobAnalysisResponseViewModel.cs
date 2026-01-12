using System.Text.Json.Serialization;

namespace InterviewService.Api.ViewModels.Response.JobDetail
{
    public class CandidateJobAnalysisResponseViewModel
    {
        public CandidateInfo CandidateInfo { get; set; } = new();
        public InterviewInfo InterviewInfo { get; set; } = new();
        public InterviewScores InterviewScores { get; set; } = new();
        public CandidateInterviewAudioRecording CandidateInterviewAudioRecording { get; set; } = new();
        public List<InterviewQuestionSummary> InterviewQuestionSummary { get; set; } = [];
        public InterviewQuestionsEvaluation QuestionsEvaluation { get; set; } = new();
        public InterviewTranscriptViewModel interviewTranscript { get; set; } = new();
    }
    public class CandidateInfo
    {
        public Guid? CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string ResumeUrl { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;            // .NET Developer
        public string InterviewStage { get; set; } = string.Empty;  // Technical Interview
        public string InterviewStatus { get; set; } = string.Empty; // Under Review
    }
    public class InterviewInfo
    {
        public Guid? InterviewId { get; set; }

        [JsonIgnore]
        public DateTime? InterviewDate { get; set; }

        [JsonIgnore]
        public DateTime? StartDateTime { get; set; }

        [JsonIgnore]
        public DateTime? EndDateTime { get; set; }

        [JsonIgnore]
        public int DurationMinutes { get; set; }

        public string DurationText => DurationMinutes > 0 ? $"{DurationMinutes} minutes" : "N/A";
        public string InterviewDateText => InterviewDate.HasValue ? InterviewDate.Value.ToString("MMM dd, yyyy")
                : "N/A";
    }
    public class InterviewScores
    {
        [JsonIgnore]
        public decimal? Technical { get; set; } = 0;

        [JsonIgnore]
        public decimal? Communication { get; set; } = 0;

        [JsonIgnore]
        public decimal? Confidence { get; set; } = 0;

        public decimal? Overall { get; set; } = 0;
        public string HiringDecision { get; set; } = string.Empty;
        public string HiringReason { get; set; } = string.Empty;

        public string? TechnicalText => $"{Technical ?? 0}/10";
        public string? CommunicationText => $"{Communication ?? 0}/10";
        public string? ConfidenceText => $"{Confidence ?? 0}/10";
    }
    public class CandidateInterviewAudioRecording
    {
        public Guid? Id { get; set; }
        public Guid? CandidateId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty; // "45 min"
    }
    public class InterviewQuestionSummary
    {
        public Guid? InterviewId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string AnswerStatus { get; set; } = string.Empty; // Not Answered | Partial | Answered
    }
    public class InterviewQuestionsEvaluation
    {
        public Guid? InterviewId { get; set; }
        public Guid? InterviewerId { get; set; }
        public Guid? CandidateId { get; set; }
        public string Type { get; set; } = "AI Interview";

        [JsonIgnore]
        public string Transcript { get; set; } = string.Empty;

        public string InterviewerName { get; set; } = string.Empty;
        public string QuestionsEvaluation { get; set; } = string.Empty;
    }

    public enum TranscriptSpeaker
    {
        Agent,
        User
    }

    public class InterviewTranscriptMessage
    {
        public TranscriptSpeaker Speaker { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsCodeBlock { get; set; }
        public string Language { get; set; } // "jsx", "js", etc
    }

    public class InterviewTranscriptViewModel
    {
        public Guid InterviewId { get; set; }
        public List<InterviewTranscriptMessage> Messages { get; set; } = new();
    }

}
