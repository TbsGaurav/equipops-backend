using Common.Services.Helper;

using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Detail;
using InterviewService.Api.ViewModels.Response.Interview_Detail;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewDetailService : IInterviewDetailService
    {
        private readonly IInterviewDetailRepository _interviewDetailRepository;
        private readonly ILogger<InterviewDetailService> _logger;
        public InterviewDetailService(IInterviewDetailRepository interviewDetailRepository, ILogger<InterviewDetailService> logger)
        {
            _interviewDetailRepository = interviewDetailRepository;
            _logger = logger;
        }

        #region Interview Detail
        public async Task<ApiResponse<InterviewCompleteResponseViewModel>> InterviewDetailCreateAsync(InterviewDetailRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _interviewDetailRepository.InterviewDetailCreateAsync(model);

            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            //if (data.id == null)
            //    Message = "Invalid data";
            //else
            //{
            Code = (int)ApiStatusCode.BadRequest;
            Status = true;
            if (model.interview_id == null)
            {
                Message = "Interview Detail is inserted successfully.";
            }
            else
                Message = "Interview Detail is updated successfully.";
            //}

            return new ApiResponse<InterviewCompleteResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewDetailResponseViewModel>> InterviewDetailAsync(Guid? interview_id, Guid? candidate_id)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Validate Input
            if (interview_id == null)
            {
                Code = (int)ApiStatusCode.BadRequest;
                Status = false;
                Message = "id cannot be empty.";
            }

            //🔹 Repository Call
            var data = await _interviewDetailRepository.InterviewDetailAsync(interview_id, candidate_id);

            if (data == null)
            {
                Code = (int)ApiStatusCode.Success;
                Message = "Invalid data.";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "Success.";
            }

            return new ApiResponse<InterviewDetailResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }

        #endregion
    }
}