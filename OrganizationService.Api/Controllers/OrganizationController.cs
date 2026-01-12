using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.Organzation;
using OrganizationService.Api.ViewModels.Request.User;

namespace OrganizationService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetOrganizations(string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetOrganizationsAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);
            return result;
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetOrganizationById(Guid Id)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetOrganizationByIdAsync(Id);
            return result;
        }

        [AllowAnonymous]
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateOrganization([FromBody] OrganizationCreateUpdateRequestViewModel request)
        {
            // 🔹 Call Service
            var result = await _organizationService.CreateUpdateOrganizationAsync(request);
            return result;
        }
        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(EmailVerifyRequestViewModel request)
        {
            // 🔹 Call Service
            var result = await _organizationService.VerifyRegistrationEmailAsync(request);
            return result;
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteOrganization([FromBody] OrganizationDeleteRequestViewModel request)
        {
            // 🔹 Call Service
            var result = await _organizationService.DeleteOrganizationAsync(request);
            return result;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetOrganizationProfile(Guid id)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetOrganizationProfileAsync(id);
            return result;
        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile(ProfileRequestViewModel profileRequest)
        {
            // 🔹 Call Service
            var result = await _organizationService.UpdateProfileAsync(profileRequest);
            return result;
        }

        [AllowAnonymous]
        [HttpGet("get-industry-deparment")]
        public async Task<IActionResult> GetIndustryDeparment(string industry)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetIndustryDeparmentAsync(industry);
            return result;
        }

        [HttpGet("all-org-list")]
        public async Task<IActionResult> GetAllOrganizations(string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetAllOrganizations(Search, Length, Page, OrderColumn, OrderDirection, IsActive);
            return result;
        }

        [HttpGet("all-org-users-by-id")]
        public async Task<IActionResult> GetAllOrganizationUsers(Guid orgId, string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetAllOrganizationUsers(orgId, Search, Length, Page, OrderColumn, OrderDirection, IsActive);
            return result;
        }

        [HttpPost("update-organization-status")]
        public async Task<IActionResult> UpdateOrganizationStatus(Guid organizationId, string action)
        {
            var result = await _organizationService.UpdateOrganizationStatusAsync(organizationId, action);
            return result;
        }

        [HttpGet("org-list-by-status")]
        public async Task<IActionResult> GetOrganizationsByStatus(string? Search, int Length = 10, int Page = 1, string OrderColumn = "name", string OrderDirection = "Asc", int? status = 0)
        {
            // 🔹 Call Service
            var result = await _organizationService.GetOrganizationsByStatus(Search, Length, Page, OrderColumn, OrderDirection, status);
            return result;
        }
    }
}
