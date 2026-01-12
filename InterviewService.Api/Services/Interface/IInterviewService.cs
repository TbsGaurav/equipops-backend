using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.ViewModels.Request.Interview;
using InterviewService.Api.ViewModels.Response.Interview;

using Microsoft.AspNetCore.Mvc;

namespace InterviewService.Api.Services.Interface
{
	public interface IInterviewService
	{
		Task<ApiResponse<InterviewListResponseViewModel>> GetInterviewsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null, Guid? OrganizationId = null);
		Task<ApiResponse<InterviewByIdResponseViewModel>> GetInterviewByIdAsync(Guid Id);
		Task<IActionResult> GetInterviewInitAsync();
		//Task<IActionResult> VerifyInterviewTokenAsync(VerifyTokenRequestViewModel model);
		Task<ApiResponse<string>> CreateInterviewTokenAsync(InterviewTokenRequestViewModel request);
		Task<ApiResponse<InterviewCreateResponseViewModel>> CreateInterviewAsync(InterviewCreateRequestViewModel model);
		Task<ApiResponse<InterviewUpdateResponseViewModel>> UpdateInterviewAsync(InterviewUpdateRequestViewModel model);
		Task<ApiResponse<InterviewListResponseViewModel>> DeleteInterviewAsync(InterviewDeleteRequestViewModel model);
		Task<IActionResult> VerifyInterviewTokenAsync(VerifyTokenRequestViewModel model);
		Task<IActionResult> RegisterCallAsync(CallRegisterRequestViewModel model);
	}
}
