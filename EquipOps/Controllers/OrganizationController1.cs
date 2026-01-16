using EquipOps.Model.Organization;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class OrganizationController1 : ControllerBase
    {
        private readonly IOrganizationService1 _organizationService;

        public OrganizationController1(IOrganizationService1 organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpPost("organizationCreate")]
        public async Task<IActionResult> OrganizationCreate([FromBody] Organization1Request request)
        {
            var result = await _organizationService.OrganizationCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("organizationList")]
        public async Task<IActionResult> GetOrganizationList(string? search = "", int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "ASC")
        {
            var result = await _organizationService.OrganizationListAsync(search, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpGet("organizationById")]
        public async Task<IActionResult> GetOrganizationById(int organization_id)
        {
            var result = await _organizationService.OrganizationByIdAsync(organization_id);
            return Ok(result);
        }

        [HttpPost("organizationDelete")]
        public async Task<IActionResult> OrganizationDelete([FromBody] Organization1DeleteRequestViewModel request)
        {
            var result = await _organizationService.OrganizationDeleteAsync(request.organization_id);
            return Ok(result);
        }

        [HttpGet("organization/dropdown")]
        public async Task<IActionResult> OrganizationDropdown()
        {
            return await _organizationService.OrganizationDropdownAsync();
        }
    }
}
