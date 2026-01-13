using EquipOps.Model.Requests.Equipment;
using EquipOps.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipOps.API.Controller
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly ILogger<EquipmentController> _logger;
        private readonly IEquipmentService _equipmentService;

        public EquipmentController(ILogger<EquipmentController> logger, IEquipmentService equipmentService)
        {
            _logger = logger;
            _equipmentService = equipmentService;
        }

        [HttpPost("create-update")]
        public async Task<IActionResult> CreateOrUpdateEquipment([FromBody] EquipmentRequest request)
        {
            _logger.LogInformation("API hit: CreateOrUpdateEquipment. Name={Name}", request.Name);

            var result = await _equipmentService.AddOrUpdateAsync(request);
            return result;
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetEquipmentById(int id)
        {
            _logger.LogInformation("API hit: GetEquipmentById. EquipmentId={EquipmentId}", id);

            var result = await _equipmentService.GetByIdAsync(id);
            return result;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetEquipmentList(
            string? search = null,
            int length = 10,
            int page = 1,
            string orderColumn = "name",
            string orderDirection = "Asc")
        {
            _logger.LogInformation(
                "API hit: GetEquipmentList | Search={Search}, Page={Page}, Length={Length}",
                search, page, length);

            var result = await _equipmentService.GetEquipmentsAsync(
                search,
                length,
                page,
                orderColumn,
                orderDirection
            );

            return result;
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteEquipment([FromBody] EquipmentRequest request)
        {
            _logger.LogInformation(
                "API hit: DeleteEquipment. EquipmentId={EquipmentId}",
                request.EquipmentId);

            var result = await _equipmentService.DeleteAsync(request.EquipmentId);
            return result;
        }
    }
}
