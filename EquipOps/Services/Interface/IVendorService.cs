using EquipOps.Model.Vendor;
using OrganizationService.Api.Helpers.ResponseHelpers.Models;

namespace EquipOps.Serives.Interface
{
	using Microsoft.AspNetCore.Mvc;
	using EquipOps.Model.Vendor;

	public interface IVendorService
	{
		Task<IActionResult> VendorCreateAsync(VendorRequest model);
		Task<IActionResult> VendorListAsync(
			string? search,
			int length,
			int page,
			string orderColumn,
			string orderDirection
		);
		Task<IActionResult> VendorByIdAsync(int vendor_id);
		Task<IActionResult> VendorDeleteAsync(int model);
	}

}
