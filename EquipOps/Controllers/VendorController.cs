using EquipOps.Model.Vendor;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers 
{
    [Route("api/[controller]")]
    //[Authorize]
    [AllowAnonymous]

    public class VendorController(IVendorService _vendorService) : ControllerBase
    {
        [HttpPost("vendorCreate")]
        public async Task<IActionResult> VendorCreate([FromBody] VendorRequest request)
        {
            var result = await _vendorService.VendorCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("vendorList")]
        public async Task<IActionResult> GetVendorList(string? search = "", int length = 10, int page = 1, string orderColumn = "name", string orderDirection = "ASC")
        {
            var result = await _vendorService.VendorListAsync(
                search,
                length,
                page,
                orderColumn,
                orderDirection
            );
            return Ok(result);
        }

        [HttpGet("vendorById")]
        public async Task<IActionResult> GetVendorById(int vendor_id)
        {
            var result = await _vendorService.VendorByIdAsync(vendor_id);
            return Ok(result);
        }

        [HttpPost("vendorDelete")]
        public async Task<IActionResult> VendorDelete(int vendor_id)
        {
            var result = await _vendorService.VendorDeleteAsync(vendor_id);
            return Ok(result);
        }
    }
}
