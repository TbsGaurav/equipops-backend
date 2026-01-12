using Dapper;

using InterviewService.Api.Helpers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Request.InterviewUpdate;

using System.Data;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewTranscriptRepository(IDbConnectionFactory _dbFactory) : IInterviewTranscriptRepository
    {
        public async Task<string> CreateInterviewTranscriptAsync(InterviewTrasncriptCreateRequestViewModel model, string? ConversationText)
        {
            try
            {
                using var conn = _dbFactory.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("p_interview_id", model.Interview_id);
                parameters.Add("p_candidate_id", model.Candidate_id);
                parameters.Add("p_interviewer_id", model.Interviewer_id);
                parameters.Add("p_conversation_text", ConversationText);
                parameters.Add("p_call_id", model.Call_id);
                parameters.Add("p_interview_update_id", model.interview_update_id);
                parameters.Add(
                    "o_id",
                    dbType: DbType.Guid,
                    direction: ParameterDirection.Output
                );

                // 🔹 Execute stored procedure
                await conn.ExecuteAsync(
                    StoreProcedure.InterviewTranscriptCreate,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var transcriptId =
                    parameters.Get<Guid?>("o_id") ?? Guid.Empty;

                return transcriptId.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        public async Task<string> UpdateInterview(TranscriptInterviewUpdateRequestViewModel model)
        {
            try
            {
                using var conn = _dbFactory.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("p_interview_id", model.interview_id);
                parameters.Add("p_candidate_id", model.candidate_id);
                parameters.Add("p_technical", model.technical);
                parameters.Add("p_communication", model.communication);
                parameters.Add("p_confidence", model.confidence);
                parameters.Add("p_overall", model.overall);
                parameters.Add("p_hiring_decision", model.hiring_decision);
                parameters.Add("p_hiring_reason", model.hiring_reason);
                parameters.Add("p_transcript", model.transcript);
                parameters.Add("p_created_by", model.created_by);
                parameters.Add("p_questions_evaluation", model.questions_evaluation);
                parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                // 🔹 Execute stored procedure
                await conn.ExecuteAsync(StoreProcedure.InterviewUpdateTranscript, parameters, commandType: CommandType.StoredProcedure);

                var id =
                    parameters.Get<Guid?>("o_id") ?? Guid.Empty;

                return id.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
