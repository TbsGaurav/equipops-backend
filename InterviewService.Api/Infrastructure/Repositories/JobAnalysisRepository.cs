using Dapper;

using InterviewService.Api.Helpers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Response.JobDetail;

using Newtonsoft.Json;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class JobAnalysisRepository : IJobAnalysisRepository
    {
        private readonly ILogger<JobAnalysisRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;

        public JobAnalysisRepository(IDbConnectionFactory dbFactory, ILogger<JobAnalysisRepository> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }


        public async Task<JobAnalysisResponseViewModel> GetJobAnalysisAsync(Guid id)
        {
            _logger.LogInformation(
                "GetJobAnalysisAsync started. InterviewId: {InterviewId}", id);

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            const string sql =
                $"SELECT {StoreProcedure.JobAnalysis}(@InterviewId)::text;";

            _logger.LogDebug("Executing SP: {StoredProcedure}", StoreProcedure.JobAnalysis);

            var json = await conn.ExecuteScalarAsync<string>(
                sql,
                new { InterviewId = id },
                transaction: tran
            );

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogWarning(
                    "GetJobAnalysisAsync returned empty result. InterviewId: {InterviewId}", id);
                return new JobAnalysisResponseViewModel();
            }

            var result = JsonConvert.DeserializeObject<JobAnalysisResponseViewModel>(json);

            _logger.LogInformation(
                "GetJobAnalysisAsync completed successfully. InterviewId: {InterviewId}", id);

            return result ?? new JobAnalysisResponseViewModel();
        }


        public async Task<CandidateJobAnalysisResponseViewModel> GetCandidateJobAnalysis(Guid id)
        {
            try
            {
                _logger.LogInformation(
                    "GetCandidateJobAnalysis started. CandidateId: {CandidateId}", id);

                using var conn = _dbFactory.CreateConnection();
                conn.Open();


                //const string sql = $"SELECT {StoreProcedure.CandidateJobAnalysis}(@Id)::text;";

                _logger.LogDebug("Executing SP: {StoredProcedure}", StoreProcedure.CandidateJobAnalysis);

                //var json = await conn.ExecuteScalarAsync<string>(
                //    sql,
                //    new { Id = id }
                //);

                var json = await conn.QuerySingleAsync<string>(
                "SELECT interviews.fn_get_candidate_interview_details(@Id)::text",
                new { Id = id }
            );


                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning(
                        "GetCandidateJobAnalysis returned empty result. CandidateId: {CandidateId}", id);
                    return new CandidateJobAnalysisResponseViewModel();
                }

                var result =
                    JsonConvert.DeserializeObject<CandidateJobAnalysisResponseViewModel>(json);

                _logger.LogInformation(
                    "GetCandidateJobAnalysis completed successfully. CandidateId: {CandidateId}", id);

                return result ?? new CandidateJobAnalysisResponseViewModel();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
