using Common.Services.Helper;
//using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Que;
using InterviewService.Api.ViewModels.Response.Interview_Que;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewQueService : IInterviewQueService
    {
        private readonly IInterviewQueRepository _interviewQueRepository;
        private readonly ILogger<InterviewQueService> _logger;
        public InterviewQueService(IInterviewQueRepository interviewQueRepository, ILogger<InterviewQueService> logger)
        {
            _interviewQueRepository = interviewQueRepository;
            _logger = logger;
        }

        #region Interview Que
        public async Task<ApiResponse<InterviewQueCreateUpdateResponseViewModel>> InterviewQueCreateAsync(InterviewQueRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _interviewQueRepository.InterviewQueCreateAsync(model);

            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            //if (data.id == null)
            //    Message = "Invalid data";
            //else
            //{
            Status = true;
            Code = (int)ApiStatusCode.Success;
            if (model.questions[0].Id == null)
                Message = "Interview Question is inserted successfully.";
            else
                Message = "Interview Question is updated successfully.";
            //}

            return new ApiResponse<InterviewQueCreateUpdateResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewQueListResponseViewModel>> InterviewQueListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Repository Call
            var data = await _interviewQueRepository.InterviewQueListAsync(search, Is_Active, length, page, orderColumn, orderDirection);

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

            return new ApiResponse<InterviewQueListResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewQueDeleteResponseViewModel>> InterviewQueDeleteAsync(InterviewQueDeleteRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _interviewQueRepository.InterviewQueDeleteAsync(model);

            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            if (data.id == null)
            {
                Code = (int)ApiStatusCode.Success;
                Message = "Invalid data";
            }
            else
            {
                Code = (int)ApiStatusCode.Success;
                Status = true;
                Message = "Interview Question is deleted successfully.";
            }

            return new ApiResponse<InterviewQueDeleteResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewQueResponseViewModel>> InterviewQueByIdAsync(Guid? id)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Validate Input
            if (id == null)
            {
                return new ApiResponse<InterviewQueResponseViewModel>
                {
                    StatusCode = (int)ApiStatusCode.BadRequest,
                    Success = false,
                    Message = "id cannot be empty."
                };
            }

            //🔹 Repository Call
            var data = await _interviewQueRepository.InterviewQueByIdAsync(id);

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

            return new ApiResponse<InterviewQueResponseViewModel>
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
