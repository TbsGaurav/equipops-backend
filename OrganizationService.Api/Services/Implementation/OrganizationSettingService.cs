using Common.Services.Helper;
using Common.Services.ViewModels.RetellAI;

using OrganizationService.Api.Helpers.EncryptionHelpers.Handlers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.OrganzationSetting;
using OrganizationService.Api.ViewModels.Response.Organization_Setting;
using OrganizationService.Api.ViewModels.Response.OrganizationSetting;

using System.Text;
using System.Text.Json;

namespace OrganizationService.Api.Services.Implementation
{
    public class OrganizationSettingService : IOrganizationSettingService
    {
        private readonly IOrganizationSettingRepository _organizationSettingRepository;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly ILogger<OrganizationSettingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RetellAIEndpoints _retellUrl;

        public OrganizationSettingService(
            IOrganizationSettingRepository organizationSettingRepository,
            ILogger<OrganizationSettingService> logger,
            EncryptionHelper encryptionHelper,
                IConfiguration config,
                 IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            RetellAIEndpoints retellUrl
        )
        {
            _organizationSettingRepository = organizationSettingRepository;
            _encryptionHelper = encryptionHelper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _config = config;
            _retellUrl = retellUrl;
        }

        public async Task<ApiResponse<List<OrganizationSettingListResponseViewModel>>> GetOrganizationSettingsAsync()
        {

            // 🔹 Repository Call
            _logger.LogInformation("Calling OrganizationSettingRepository.GetOrganizationSettingsAsync.");

            var data = await _organizationSettingRepository.GetOrganizationSettingsAsync();

            // 🔹 Success
            _logger.LogInformation("Organization Settings retrieved successfully.");

            return new ApiResponse<List<OrganizationSettingListResponseViewModel>>
            {
                Success = true,
                Message = "Organization Settings retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<OrganizationSettingByKeyResponseViewModel>> GetOrganizationSettingByKeyAsync(string Key)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling OrganizationSettingRepository.GetOrganizationSettingByKeyAsync.");

            var data = await _organizationSettingRepository.GetOrganizationSettingByKeyAsync(Key);

            // 🔹 Success
            _logger.LogInformation("Organization Setting retrieved successfully.");

            return new ApiResponse<OrganizationSettingByKeyResponseViewModel>
            {
                Success = true,
                Message = "Organization Setting retrieved successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<OrganizationSettingCreateUpdateResponseViewModel>> CreateUpdateOrganizationSettingAsync(OrganizationSettingCreateUpdateRequestViewModel model)
        {
            _logger.LogInformation("OrganizationSettingService: CreateUpdateOrganizationSettingAsync START. Key={Key}", model.Key);

            // 🔹 Validate Input
            if (string.IsNullOrEmpty(model.Key)
                || string.IsNullOrEmpty(model.Value))
            {
                _logger.LogWarning(
                "Validation failed: Required fields are missing. Key={Key}",
                model.Key
            );

                return new ApiResponse<OrganizationSettingCreateUpdateResponseViewModel>
                {
                    Success = false,
                    Message = "Organization, Key & Value is required."
                };
            }

            var nonEncryptedKeys = new[]
            {
                    "is_email_notification",
                    "is_mobile_notification",
                    "two_factor_auth"
                };

            // 🔹 Repository Call
            _logger.LogInformation("Calling OrganizationSettingRepository.CreateUpdateOrganizationSettingAsync for Key={Key}", model.Key);

            // 🔹 Encrypt Value
            if (!nonEncryptedKeys.Contains(model.Key))
            {
                model.Value = _encryptionHelper.EncryptForReact(model.Value);
            }

            var result = await _organizationSettingRepository.CreateUpdateOrganizationSettingAsync(model);

            var data = result.Data;
            var isExisting = result.Exists;


            // 🔹 Success
            _logger.LogInformation("Organization Setting created successfully. Key={Key}", model.Key);

            return new ApiResponse<OrganizationSettingCreateUpdateResponseViewModel>
            {
                Success = true,
                Message = isExisting
                        ? "Organization Setting updated successfully."
                        : "Organization Setting created successfully.",
                Data = data
            };
        }

        public async Task<ApiResponse<OrganizationSettingCreateUpdateRequestViewModel>> Create_retell_llm_key()
        {
            var orgIdClaim = _httpContextAccessor.HttpContext?
                             .User?
                             .FindFirst("organization_id")?
                             .Value;

            if (string.IsNullOrEmpty(orgIdClaim))
                throw new Exception("Organization ID not found in token.");

            Guid organizationId = Guid.Parse(orgIdClaim);

            // fetch retell api key from organization settings
            var apiKey = await _organizationSettingRepository.GetRetellAiKey(organizationId);
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("Retell AI Key is not configured for this organization.");

            // create retell llm key
            var client = _httpClientFactory.CreateClient();
            string beginMessage = "Hi! Welcome to your interview. Let’s start with a quick introduction — could you tell me a bit about yourself?";


            #region static agent prompt
            const string RETELL_AGENT_GENERAL_PROMPT = @"
               You are an interviewer who is an expert in asking follow-up questions to uncover deeper insights.
               You have to keep the interview for {{mins}} or short.

               The name of the person you are interviewing is {{name}}.

               The interview objective is {{objective}}.

                LANGUAGE HANDLING RULES:
                    ================================
                    MULTILINGUAL UNDERSTANDING (CRITICAL)
                    ================================

                    - The user may speak in ANY language, including Hindi or mixed languages.
                    - You MUST correctly understand and interpret the user’s response
                      regardless of the language spoken.
                    - You MUST internally translate any non-English input into English before reasoning.
                    - You MUST NEVER say that you do not understand due to language.
                    - You MUST NEVER ask the user to switch languages.
                    - You MUST ALWAYS respond ONLY in English.
                    - You MUST NEVER mix multiple languages in a single response.
                    - If no language is provided, the interview MUST start in English.
                    - If the user explicitly asks the agent to switch languages (e.g., ""Can you speak Hindi?""),
                      you MUST politely refuse and continue responding in the initially selected agent language.
                    - The refusal MUST be brief, professional, and must NOT mention system rules or policies.
                    - You MUST ALWAYS respond in the initially selected agent language.
                    - You MAY understand and process user responses in any language.
                    - Even if the user switches languages, DO NOT change the agent’s response language.
                    - NEVER ask the user to confirm or choose a language.
                    - NEVER mix multiple languages in a single response.
                    - Maintain the same professional tone and interview structure in all responses.
                 TURN-TAKING & INTERRUPTION RULES (CRITICAL):
                ================================
                - You MUST NEVER interrupt the user.
                - You MUST wait until the user has COMPLETELY finished speaking.
                - Short pauses do NOT indicate completion.
                - Respond ONLY after clear completion.
                - When responding, acknowledge briefly, then ask the next question.

               These are some of the questions you can ask:
               {{questions}}

               Once you ask a question, make sure you ask a follow-up question on it.

               Follow the guidelines below when conversing:
               - Follow a professional yet friendly tone.
               - Ask precise and open-ended questions.
               - The question word count should be 30 words or less.
               - Make sure you do not repeat any of the questions.
               - Do not talk about anything not related to the objective and the given questions.
               - If the name is given, use it in the conversation.

               When the interview is completed or you decide to end the call:
               - Politely thank the candidate.
               - Inform them that the HR team will contact them regarding the next steps.
               - Then end the call using the end_call tool.
               ";
            #endregion

            var payload = new
            {
                model = "gpt-4.1",
                start_speaker = "agent",
                begin_message = beginMessage,
                general_prompt = RETELL_AGENT_GENERAL_PROMPT,
                begin_after_user_silence_ms = 12000,
                general_tools = new[]
                {
                    new
                    {
                        type = "end_call",
                        name = "end_call_1",
                        description =
                            "Before ending the call, thank the candidate and inform them that HR will contact them about the next steps. Then end the call.'"
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);

            var requestMessage = new HttpRequestMessage(
                     HttpMethod.Post,
                     _retellUrl.CreateRetellLLM)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await client.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to generate Retell LLM Key: {errorMsg}");
            }

            var retellKey = await response.Content.ReadAsStringAsync();
            var retellResponse = JsonSerializer.Deserialize<Retell_llm_response_view_model>(retellKey);

            if (retellResponse == null || string.IsNullOrWhiteSpace(retellResponse.llm_id))
                throw new Exception("Retell AI did not return a valid llm_id.");

            // store encrypted llm key in organization settings
            var encryptedApiKey = _encryptionHelper.EncryptForReact(retellResponse.llm_id);
            var request = new OrganizationSettingCreateUpdateRequestViewModel
            {
                Key = "retell_llm_key",
                Value = encryptedApiKey
            };

            // 🔹 Repository Call
            await _organizationSettingRepository.CreateUpdateOrganizationSettingAsync(request);

            return new ApiResponse<OrganizationSettingCreateUpdateRequestViewModel>
            {
                Success = true,
                Message = "Retell LLM key created successfully.",
                Data = request
            };
        }
    }
}
