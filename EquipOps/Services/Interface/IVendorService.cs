using Microsoft.AspNetCore.Mvc;
using EquipOps.Model.Vendor;

namespace EquipOps.Services.Interface
{
	public interface IVendorService
	{
		Task<IActionResult> VendorCreateUpdateAsync(VendorRequest request);
		Task<IActionResult> VendorListAsync(string? search,int length,int page,string orderColumn,string orderDirection);
		Task<IActionResult> VendorByIdAsync(int vendor_id);
		Task<IActionResult> VendorDeleteAsync(int id);
	}
}
