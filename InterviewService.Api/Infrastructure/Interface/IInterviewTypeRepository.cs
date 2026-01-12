using InterviewService.Api.ViewModels.Request.Interview_Type;
using InterviewService.Api.ViewModels.Response.Interview;

namespace InterviewService.Api.Infrastructure.Interface
{
    public interface IInterviewTypeRepository
    {
        Task<InterviewTypeListResponseViewModel> GetInterviewTypesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<InterviewTypeByIdResponseViewModel> GetInterviewTypeByIdAsync(Guid Id);
        Task<InterviewTypeCreateUpdateResponseViewModel> CreateUpdateInterviewTypeAsync(InterviewTypeCreateUpdateRequestViewModel request);
        Task DeleteInterviewTypeAsync(InterviewTypeDeleteRequestViewModel request);
    }
}
