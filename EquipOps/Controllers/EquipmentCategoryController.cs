using CommonHelper.Enums;
using CommonHelper.ResponseHelpers.Handlers;
using EquipOps.Model.EquipmentCategory;
using EquipOps.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class EquipmentCategoryController(ILogger<EquipmentCategoryController> logger, IEquipmentCategoryService _equipmentcategoryService) : ControllerBase
    {
        [HttpPost("equipmentCategoryCreateUpdate")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] EquipmentCategoryRequest request, [FromServices] IValidator<EquipmentCategoryRequest> validator)
        {
            logger.LogInformation("API hit: CreateOrUpdate. Name={Name}", request.category_name);

            var result = await validator.ValidateAsync(request);
            if (!result.IsValid)
            {
                var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ResponseHelper<object>.Error(message: "Validation failed.", errors: errors, statusCode: StatusCodeEnum.BAD_REQUEST));
            }
            return await _equipmentcategoryService.EquipmentCategoryCreateAsync(request);
        }

        [HttpGet("equipmentCategoryList")]
        public async Task<IActionResult> GetList(string? search = "", int length = 10, int page = 1, string orderColumn = "category_name", string orderDirection = "ASC")
        {
            var result = await _equipmentcategoryService.EquipmentCategoryListAsync(search, length, page, orderColumn, orderDirection);
            return Ok(result);
        }

        [HttpGet("equipmentCategoryById")]
        public async Task<IActionResult> GetById(int category_id)
        {
            var result = await _equipmentcategoryService.EquipmentCategoryByIdAsync(category_id);
            return Ok(result);
        }

        [HttpPost("equipmentCategoryDelete")]
        public async Task<IActionResult> Delete([FromBody] EquipmentCategoryDeleteRequestViewModel request)
        {
            var result = await _equipmentcategoryService.EquipmentCategoryDeleteAsync(request.category_id);
            return Ok(result);
        }
    }
}
