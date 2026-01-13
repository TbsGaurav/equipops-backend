using EquipOps.Model.EquipmentCategory;
using EquipOps.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EquipmentCategoryController : ControllerBase
    {
        private readonly IEquipmentCategoryService _service;

        public EquipmentCategoryController(IEquipmentCategoryService service)
        {
            _service = service;
        }

        [HttpPost("equipmentCategoryCreateUpdate")]
        public async Task<IActionResult> Create([FromBody] EquipmentCategoryRequest request)
        {
            var result = await _service.EquipmentCategoryCreateAsync(request);
            return Ok(result);
        }

        [HttpGet("equipmentCategoryList")]
        public async Task<IActionResult> GetList(string? search = "", int length = 10, int page = 1, string orderColumn = "category_name", string orderDirection = "ASC")
        {
            var result = await _service.EquipmentCategoryListAsync(search, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpGet("equipmentCategoryById")]
        public async Task<IActionResult> GetById(int category_id)
        {
            var result = await _service.EquipmentCategoryByIdAsync(category_id);
            return Ok(result);
        }

        [HttpPost("equipmentCategoryDelete")]
        public async Task<IActionResult> Delete([FromBody] EquipmentCategoryDeleteRequestViewModel request)
        {
            var result = await _service.EquipmentCategoryDeleteAsync(request.category_id);
            return Ok(result);
        }
    }
}
