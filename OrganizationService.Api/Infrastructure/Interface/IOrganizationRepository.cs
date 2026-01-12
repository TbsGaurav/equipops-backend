using OrganizationService.Api.ViewModels.Request.Organzation;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.Organization;
using OrganizationService.Api.ViewModels.Response.User;

namespace OrganizationService.Api.Infrastructure.Interface
{
    public interface IOrganizationRepository
    {
        Task<OrganizationListResponseViewModel> GetOrganizationsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<OrganizationByIdResponseViewModel> GetOrganizationByIdAsync(Guid Id);
        Task<OrganizationCreateUpdateResponseViewModel> CreateUpdateOrganizationAsync(OrganizationCreateUpdateRequestViewModel request);
        Task<EmailVerifyResponseViewModel> VerifyRegistrationEmailAsync(EmailVerifyRequestViewModel request, string Email, Guid UserId);
        Task DeleteOrganizationAsync(OrganizationDeleteRequestViewModel request);
        Task<ProfileResponseViewModel> GetOrganizationProfileAsync(Guid id);
        Task<string?> GetExistingProfilePhotoAsync(Guid userId);
        Task UpdateProfileAsync(ProfileRequestViewModel model);
        Task<OrganizationListResponseViewModel> GetAllOrganizationsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);
        Task<UserListResponseViewModel> GetAllOrganizationUsers(Guid orgId,
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null);

        Task<(bool Success, string Message)> UpdateOrganizationStatusAsync(Guid organizationId, string action);
        Task<OrganizationListResponseViewModel> GetOrganizationsByStatus(
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", int? status = 0);
    }
}
