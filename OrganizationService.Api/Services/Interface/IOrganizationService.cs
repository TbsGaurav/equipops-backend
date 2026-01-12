using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.ViewModels.Request.Organzation;
using OrganizationService.Api.ViewModels.Request.User;

namespace OrganizationService.Api.Services.Interface
{
    public interface IOrganizationService
    {
        Task<IActionResult> GetOrganizationsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<IActionResult> GetOrganizationByIdAsync(Guid Id);
        Task<IActionResult> CreateUpdateOrganizationAsync(OrganizationCreateUpdateRequestViewModel model);
        Task<IActionResult> VerifyRegistrationEmailAsync(EmailVerifyRequestViewModel model);
        Task<IActionResult> DeleteOrganizationAsync(OrganizationDeleteRequestViewModel model);
        Task<IActionResult> GetOrganizationProfileAsync(Guid id);
        Task<IActionResult> UpdateProfileAsync(ProfileRequestViewModel model);
        Task<IActionResult> GetIndustryDeparmentAsync(string industry);
        Task<IActionResult> GetAllOrganizations(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<IActionResult> GetAllOrganizationUsers(Guid orgId, string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null);
        Task<IActionResult> UpdateOrganizationStatusAsync(Guid organizationId, string action);
        Task<IActionResult> GetOrganizationsByStatus(string? search, int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "Asc", int? isActive = 0);
    }
}
