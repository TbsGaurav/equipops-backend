using Microsoft.AspNetCore.Mvc;

using SettingService.Api.Services.Interface;
using SettingService.Api.ViewModels.Request.EmailTemplate;

namespace SettingService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplateController(ILogger<SubscriptionController> logger, IEmailTemplateService emailTemplateService) : ControllerBase
    {

        #region Email Template
        [HttpPost("createUpdate")]
        public async Task<IActionResult> CreateOrUpdateEmailTemplate(EmailTemplateCreateUpdateRequestViewModel request)
        {
            var result = await emailTemplateService.CreateUpdateEmailTemplateAsync(request);

            return Ok(result);
        }

        [HttpGet("EmailTemplateList")]
        public async Task<IActionResult> GetEmailTemplateList(string? search = "", bool? IsActive = null, int length = 10, int page = 1, string orderColumn = "type", string orderDirection = "ASC")
        {
            var result = await emailTemplateService.EmailTemplateListAsync(search, IsActive, length, page, orderColumn, orderDirection);

            return Ok(result);
        }

        [HttpPost("EmailTemplateDelete")]
        public async Task<IActionResult> EmailTemplateDelete([FromBody] EmailTemplateDeleteRequestViewModel request)
        {
            var result = await emailTemplateService.EmailTemplateDeleteAsync(request);
            return Ok(result);
        }

        [HttpGet("EmailTemplateById")]
        public async Task<IActionResult> GetEmailTemplate_ById(Guid id)
        {
            var result = await emailTemplateService.EmailTemplateByIdAsync(id);

            return Ok(result);
        }

        #endregion 
    }
}
