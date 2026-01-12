using Common.Services.Helper;

using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview_Form;
using InterviewService.Api.ViewModels.Response.Interview_Form;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewFormService : IInterviewFormService
    {
        private readonly IInterviewFormRepository _interviewFormRepository;
        private readonly ILogger<InterviewFormService> _logger;
        public InterviewFormService(IInterviewFormRepository interviewFormRepository, ILogger<InterviewFormService> logger)
        {
            _interviewFormRepository = interviewFormRepository;
            _logger = logger;
        }

        #region Interview Form
        public async Task<ApiResponse<InterviewFormCreateUpdateResponseViewModel>> InterviewFormCreateAsync(InterviewFormRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _interviewFormRepository.InterviewFormCreateAsync(model);

            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;

            //if (data.id == null)
            //    Message = "Invalid data";
            //else
            //{
            Status = true;
            Code = (int)ApiStatusCode.Success;
            if (model.id == null)
                Message = "Interview form is inserted successfully.";
            else
                Message = "Interview form is updated successfully.";
            //}

            return new ApiResponse<InterviewFormCreateUpdateResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewFormListResponseViewModel>> InterviewFormListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Repository Call
            var data = await _interviewFormRepository.InterviewFormListAsync(search, Is_Active, length, page, orderColumn, orderDirection);

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

            return new ApiResponse<InterviewFormListResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewFormDeleteResponseViewModel>> InterviewFormDeleteAsync(InterviewFormDeleteRequestViewModel model)
        {
            //🔹 Repository Call

            var data = await _interviewFormRepository.InterviewFormDeleteAsync(model);

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
                Message = "Interview form is deleted successfully.";
            }

            return new ApiResponse<InterviewFormDeleteResponseViewModel>
            {
                StatusCode = Code,
                Success = Status,
                Message = Message,
                Data = data
            };
        }
        public async Task<ApiResponse<InterviewFormResponseViewModel>> InterviewFormByIdAsync(Guid? id)
        {
            string Message = "";
            bool Status = false;
            int Code = (int)ApiStatusCode.BadRequest;
            //🔹 Validate Input
            if (id == null)
            {
                return new ApiResponse<InterviewFormResponseViewModel>
                {
                    StatusCode = (int)ApiStatusCode.BadRequest,
                    Success = false,
                    Message = "id cannot be empty."
                };
            }

            //🔹 Repository Call
            var data = await _interviewFormRepository.InterviewFormByIdAsync(id);

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

            return new ApiResponse<InterviewFormResponseViewModel>
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
