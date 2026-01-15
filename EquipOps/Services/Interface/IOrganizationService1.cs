using EquipOps.Model.Organization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Services.Interface
{
    public interface IOrganizationService1
    {
        Task<IActionResult> OrganizationCreateAsync(Organization1Request request);
        Task<IActionResult> OrganizationByIdAsync(int organization_id);
        Task<IActionResult> OrganizationDeleteAsync(int organization_id);
        Task<IActionResult> OrganizationListAsync(string? search,int length,int page,string orderColumn,string orderDirection
        );
    }
}
