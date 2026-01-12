namespace Common.Services.ViewModels.InterviewEvaluation
{
    public class InterviewTranscriptRequest
    {
        public string Transcript { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
    }
    public class InterviewEvaluationResponse
    {
        // Per-question evaluation
        public List<QuestionEvaluation> Evaluations { get; set; } = new();

        // Aggregated overall scores
        public OverallSkillScores OverallScores { get; set; } = new();

        public string HiringDecision { get; set; } = string.Empty;
        public string HiringReason { get; set; } = string.Empty;
    }

    public class QuestionEvaluation
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        // Must be: Correct | Partial | Not Answered
        public string Answer_Status { get; set; } = string.Empty;

        // Per-question scores (0–10)
        public int Technical { get; set; }
        public int Communication { get; set; }
        public int Confidence { get; set; }
    }

    public class OverallSkillScores
    {
        public double Technical { get; set; }
        public double Communication { get; set; }
        public double Confidence { get; set; }

        public double Overall { get; set; }
        public double Percentage { get; set; }
    }
}
