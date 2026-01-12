using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Detail;
using InterviewService.Api.ViewModels.Response.Interview_Detail;

using SettingService.Api.Helpers;

using System.Data;
using System.Security.Claims;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class InterviewDetailRepository : IInterviewDetailRepository
    {
        private readonly ILogger<InterviewDetailRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly PgHelper _pghelper;

        public InterviewDetailRepository(IDbConnectionFactory dbFactory, ILogger<InterviewDetailRepository> logger, IHttpContextAccessor contextAccessor, PgHelper pghelper)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _pghelper = pghelper;
        }

        #region Interview Detail
        public async Task<InterviewCompleteResponseViewModel> InterviewDetailCreateAsync(InterviewDetailRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            var param = new Dictionary<string, DbParam>
            {
                // output params
                { "_return_id", new DbParam { Value = null, DbType = DbType.Guid, Direction = ParameterDirection.InputOutput } },

                { "id", new DbParam { Value = null, DbType = DbType.Guid } },
                { "interview_id", new DbParam { Value = request.interview_id, DbType = DbType.Guid } },
                { "candidate_id", new DbParam { Value = request.candidate_id, DbType = DbType.Guid } },
                { "name", new DbParam { Value = request.name, DbType = DbType.String } },
                { "interview_date", new DbParam { Value = request.interview_date, DbType = DbType.DateTime} },
                { "overall_score", new DbParam { Value = request.overall_score, DbType = DbType.Decimal } },
                { "communication_score", new DbParam { Value = request.communication_score, DbType = DbType.Decimal } },
                { "description", new DbParam { Value = request.description, DbType = DbType.Guid } },
                { "record_url", new DbParam { Value = request.record_url, DbType = DbType.Guid } },
                { "_updated_by", new DbParam { Value = updated_by, DbType = DbType.Guid } },
            };

            dynamic result = await _pghelper.CreateUpdateAsync("master.sp_user_role_create_update", param);

            // Map output parameters to entity
            var data = new InterviewCompleteResponseViewModel
            {
                //id = result._return_id,
                //name = request.name,
                //is_delete = false,
                //is_active = true,
                //created_by = updated_by,
                //created_date = result._return_createddate,
                //updated_by = updated_by,
                //updated_date = result._return_updateddate,
            };
            // Return in ResponseViewModel wrapper
            return data;
        }
        public async Task<InterviewDetailResponseViewModel> InterviewDetailAsync(Guid? interview_id, Guid? candidate_id)
        {
            var Params = new Dictionary<string, DbParam>
            {
                { "_interivew_id", new DbParam { Value = interview_id ?? null, DbType = DbType.Guid } },
                { "_candidate_id", new DbParam { Value = candidate_id ?? null, DbType = DbType.Guid } },
                { "ref_interview_udpate", new DbParam { Value = "mycursor_iu", DbType = DbType.String, Direction = ParameterDirection.InputOutput } },
                { "ref_interview_transcript", new DbParam { Value = "mycursor_it", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
            };

            dynamic response = await _pghelper.ListAsync("interviews.sp_interview_detail_get", Params);

            InterviewDetailResponseViewModel interviewDetailResponseViewModel = new InterviewDetailResponseViewModel();
            //dynamic obj = response.ref_interview_udpate[0];
            //List<dynamic> list = new List<dynamic>();
            foreach (var row in response.ref_interview_udpate)  // row is dynamic ExpandoObject
            {
                interviewDetailResponseViewModel.interviewDetail.Add(new InterviewDetail
                {
                    id = row.id,
                    interview_id = row.interview_id,
                    candidate_id = row.candidate_id,
                    name = row.name,
                    interview_date = row.interview_date,
                    overall_score = row.overall_score,
                    communication_score = row.communication_score,
                    description = row.description,
                    record_url = row.record_url,
                    created_by = row.created_by,
                    created_date = row.created_date,
                    updated_by = row.updated_by,
                    updated_date = row.updated_date,
                    is_delete = row.is_delete,
                    is_active = row.is_active
                });
            }
            foreach (var row in response.ref_interview_transcript)  // row is dynamic ExpandoObject
            {
                interviewDetailResponseViewModel.interviewtranscript.Add(new InterviewTranscript
                {
                    id = row.id,
                    interview_id = row.interview_id,
                    candidate_id = row.candidate_id,
                    interviewer_id = row.interviewer_id,
                    interview_update_id = row.interview_update_id,
                    interviewer_text = row.interviewer_text,
                    candidate_text = row.candidate_text,
                    created_by = row.created_by,
                    created_date = row.created_date,
                    updated_by = row.updated_by,
                    updated_date = row.updated_date,
                    is_delete = row.is_delete,
                    is_active = row.is_active
                });
            }

            return interviewDetailResponseViewModel;
        }

        #endregion
    }
}
