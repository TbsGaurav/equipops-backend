using Common.Services.ViewModels.RetellAI;

using InterviewService.Api.Helpers.ResponseHelpers.Enums;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interviewer;
using InterviewService.Api.ViewModels.Response.Interviewer;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Text;

namespace InterviewService.Api.Services.Implementation
{
    public class InterviewerService(ILogger<InterviewerService> _logger, IInterviewerRepository interviewerRepository, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, RetellAIEndpoints _retellUrls) : IInterviewerService
    {
        private readonly string _uploadPath = configuration["ImageUploadSettings:InterviewerUploadFolderPath"] ?? "Uploads/Interviewer";
        private readonly string _gatewayBaseUrl = configuration["ImageUploadSettings:BaseUrl"];

        #region Create Interviewer 
        public async Task<IActionResult> CreateInterviewerAsync(InterviewerCreateRequestViewModel model)
        {
            _logger.LogInformation("InterviewerService: CreateInterviewerAsync START. Voice_id={Voice_id}", model.Voice_id);

            // 🔹 Validation
            if (string.IsNullOrWhiteSpace(model.Voice_id))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Voice ID is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Interviewer Name is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            // 🔹 Fetch organization id from token
            var orgIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Organization ID not found in token.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            Guid organizationId = Guid.Parse(orgIdClaim);

            _logger.LogInformation("Calling InterviewerRepository.CreateInterviewerAsync for Name={Name}", model.Name);

            // 🔹 Fetch Retell API key
            var retellKey = await interviewerRepository.GetRetellAiKey(organizationId);

            if (string.IsNullOrEmpty(retellKey))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "RetellAI API Key is not configured for this organization.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", retellKey);
            http.DefaultRequestHeaders.Add("Accept", "application/json");

            // 🔹 Fetch LLM key
            var llmKey = await interviewerRepository.Get_retell_LLM_key(organizationId);

            var requestPayload = new AgentCreateRequestViewModel
            {
                ResponseEngine = new AgentData
                {
                    LlmId = llmKey,
                    Type = "retell-llm"
                },
                VoiceId = model.Voice_id,
                AgentName = model.Name,
                SystemPrompt = "You are a helpful voice assistant.",
                Language = "en-US",
                Interruptible = true,
                EndCallOnGoodbye = true
            };

            var json = JsonConvert.SerializeObject(requestPayload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            // 🔹 Call Retell Create-Agent API
            var response = await http.PostAsync(_retellUrls.CreateAgent, httpContent);
            response.EnsureSuccessStatusCode();

            var resultJson = await response.Content.ReadAsStringAsync();
            var agentResponse = JsonConvert.DeserializeObject<RetellCreateAgentResponse>(resultJson);

            if (agentResponse == null || string.IsNullOrEmpty(agentResponse.Agent_id))
            {
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Failed to create Retell agent.",
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }

            var agentId = agentResponse.Agent_id;
            // 🔹 Fetch voice data
            var voiceData = await interviewerRepository.GetVoiceById(model.Voice_id, retellKey);

            // 🔹 Avatar file name
            string avatarFileName = null;
            if (model.Avatar != null)
            {
                var ext = Path.GetExtension(model.Avatar.FileName);
                avatarFileName = $"{Guid.NewGuid()}{ext}";
            }

            // 🔹 DB create request
            var request = new InterviewerDataCreateRequestViewModel
            {
                Name = model.Name,
                Agent_id = agentId,
                Voice_id = model.Voice_id,
                Avatar_url = avatarFileName,
                Record_url = voiceData?.Preview_audio_url,
                Organization_id = organizationId
            };

            var data = await interviewerRepository.CreateInterviewerAsync(request);

            if (data == null || data.Id == Guid.Empty)
            {
                return new ConflictObjectResult(
                    ResponseHelper<string>.Error(
                        "Interviewer creation failed.",
                        statusCode: StatusCodeEnum.CONFLICT_OCCURS
                    )
                );
            }

            // 🔹 Save avatar & generate public URL
            if (model.Avatar != null)
            {
                string interviewerIdFolder = data.Id.ToString();
                await SaveAvatarFile(model.Avatar, interviewerIdFolder, avatarFileName);
                data.Avatar_url = GenerateImageUrl(interviewerIdFolder, avatarFileName);
            }

            _logger.LogInformation("Interviewer created successfully. Id={Id}, Name={Name}", data.Id, model.Name);

            return new OkObjectResult(
                ResponseHelper<InterviewerCreateUpdateResponseViewModel>.Success(
                    "Interviewer created successfully.",
                    data
                )
            );
        }

        #endregion

        #region Delete Interviewer
        public async Task<IActionResult> DeleteInterviewerAsync(InterviewerDeleteRequestViewModel model)
        {
            _logger.LogInformation("InterviewerService: DeleteInterviewerAsync START. Id={Id}", model?.Id);

            // 🔹 Validate input
            if (model == null || model.Id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid interviewer ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            // 🔹 Get interviewer details
            var interviewer = await interviewerRepository.GetInterviewerByIdAsync(model.Id);
            if (interviewer == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Interviewer not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            var agentId = interviewer.Agent_id;

            // 🔹 Get organization ID
            var orgIdClaim = httpContextAccessor.HttpContext?
                .User?
                .FindFirst("organization_id")?
                .Value;

            if (string.IsNullOrEmpty(orgIdClaim))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Organization ID missing in token.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            Guid organizationId = Guid.Parse(orgIdClaim);

            // 🔹 Get Retell API key
            var apiKey = await interviewerRepository.GetRetellAiKey(organizationId);
            if (string.IsNullOrEmpty(apiKey))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "RetellAI API Key is not configured for this organization.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            // 🔹 Delete Retell agent (if exists)
            if (!string.IsNullOrWhiteSpace(agentId))
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                var url = _retellUrls.DeleteAgent(agentId);
                var retellResponse = await http.DeleteAsync(url);

                retellResponse.EnsureSuccessStatusCode();

                _logger.LogInformation(
                    "Retell agent deleted successfully. AgentId={AgentId}",
                    agentId
                );
            }

            // 🔹 Delete interviewer from DB
            await interviewerRepository.DeleteInterviewerAsync(model);

            _logger.LogInformation("Interviewer deleted successfully. Id={Id}", model.Id);

            return new OkObjectResult(
                ResponseHelper<string>.Success(
                    "Interviewer deleted successfully."
                )
            );
        }
        #endregion

        #region get voice list from retell ai
        public async Task<IActionResult> GetVoices()
        {
            var orgIdClaim = httpContextAccessor.HttpContext?
                  .User?
                  .FindFirst("organization_id")?
                  .Value;

            if (string.IsNullOrEmpty(orgIdClaim))
            {
                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Organization ID not found in token.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            Guid organizationId = Guid.Parse(orgIdClaim);
            var apiKey = await interviewerRepository.GetRetellAiKey(organizationId);

            if (string.IsNullOrEmpty(apiKey))
            {
                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                "RetellAI API Key is not configured for this organization.",
                statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }
            using var client = new HttpClient();

            client.BaseAddress = new Uri(_retellUrls.BaseUrl);

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            // call retell get-voices api
            var response = await client.GetAsync(_retellUrls.GetVoices);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<List<RetellAiVoiceModel>>(json);
            return new OkObjectResult(ResponseHelper<List<RetellAiVoiceModel>>.Success(
                "Voices retrieved successfully.",
                result
            ));
        }
        #endregion

        #region Get Interviewer By Id
        public async Task<IActionResult> GetInterviewerByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid interviewer ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            _logger.LogInformation("Calling InterviewerRepository.GetInterviewerByIdAsync. Id={Id}", id);

            var data = await interviewerRepository.GetInterviewerByIdAsync(id);

            if (data == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Interviewer not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            // 🔹 Avatar URL
            if (!string.IsNullOrEmpty(data.Avatar_url))
            {
                data.Avatar_url = GenerateImageUrl(data.Id.ToString(), data.Avatar_url);
            }
            // 🔹 Voice data (repository call)
            if (!string.IsNullOrWhiteSpace(data.Voice_id))
            {
                var orgIdClaim = httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst("organization_id")?
                    .Value;

                if (!string.IsNullOrEmpty(orgIdClaim))
                {
                    var organizationId = Guid.Parse(orgIdClaim);
                    var apiKey = await interviewerRepository.GetRetellAiKey(organizationId);

                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        var voice = await interviewerRepository.GetVoiceById(data.Voice_id, apiKey);
                        data.Voice_data = voice;
                    }
                }
            }
            _logger.LogInformation("Interviewer retrieved successfully. Id={Id}", id);
            return new OkObjectResult(
                ResponseHelper<InterviewerListResponseViewModel>.Success(
                    "Interviewer retrieved successfully.",
                    data
                )
            );
        }

        #endregion

        #region Get Interviewer List
        public async Task<IActionResult> GetInterviewersAsync(Guid? organizationId)
        {
            _logger.LogInformation("Calling InterviewerRepository.GetInterviewersAsync.");

            var user = httpContextAccessor.HttpContext?.User;

            Guid? finalOrganizationId = null;
            if (organizationId.HasValue)
            {
                finalOrganizationId = organizationId.Value;
            }
            else
            {
                var orgIdClaim = user?
                    .FindFirst("organization_id")?
                    .Value;

                if (!string.IsNullOrWhiteSpace(orgIdClaim)
                    && Guid.TryParse(orgIdClaim, out var parsedOrgId))
                {
                    finalOrganizationId = parsedOrgId;
                }
            }
            var data = await interviewerRepository.GetInterviewersAsync(finalOrganizationId);
            foreach (var item in data)
            {
                if (!string.IsNullOrEmpty(item.Avatar_url))
                {
                    item.Avatar_url = GenerateImageUrl($"{item.Id}", item.Avatar_url);
                }
            }
            // 🔹 Success
            _logger.LogInformation("Interviewer retrieved successfully.");
            return new OkObjectResult(ResponseHelper<List<InterviewerData>>.Success(
               "Interviewers retrieved successfully.",
                data
            ));
        }
        #endregion

        #region generate image url
        private string GenerateImageUrl(string folderName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath, folderName, fileName);
            if (!System.IO.File.Exists(physicalPath))
                return string.Empty;

            var publicPath = $"interview/uploads/Interviewer/{folderName}/{fileName}".Replace("\\", "/");

            return $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";
        }
        #endregion

        #region Get Voice By Id from retell ai
        public async Task<IActionResult> GetVoiceById(string VoiceId)
        {
            var orgIdClaim = httpContextAccessor.HttpContext?
                  .User?
                  .FindFirst("organization_id")?
                  .Value;

            if (string.IsNullOrEmpty(orgIdClaim))
            {
                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                        "Organization ID not found in token.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }
            Guid organizationId = Guid.Parse(orgIdClaim);
            var apiKey = await interviewerRepository.GetRetellAiKey(organizationId);

            if (string.IsNullOrEmpty(apiKey))
            {
                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                "RetellAI API Key is not configured for this organization.",
                statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }
            var response = await interviewerRepository.GetVoiceById(VoiceId, apiKey);

            return new OkObjectResult(ResponseHelper<RetellAiVoiceModel>.Success(
                "Voice retrieved successfully.",
                response
            ));
        }
        #endregion

        #region Update Interviewer
        public async Task<IActionResult> UpdateInterviewerAsync(InterviewerUpdateRequestViewModel model)
        {
            _logger.LogInformation("InterviewerService: UpdateInterviewerAsync START. Id={Id}", model?.Id);

            // 🔹 Validation
            if (model == null || model.Id == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Invalid Interviewer ID.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Interviewer Name is required.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            // 🔹 Fetch org ID
            var orgIdClaim = httpContextAccessor.HttpContext?
                .User?
                .FindFirst("organization_id")?
                .Value;

            if (string.IsNullOrEmpty(orgIdClaim))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "Organization ID not found in token.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            Guid organizationId = Guid.Parse(orgIdClaim);

            // 🔹 Fetch existing interviewer
            var existing = await interviewerRepository.GetInterviewerByIdAsync(model.Id.Value);
            if (existing == null)
            {
                return new NotFoundObjectResult(
                    ResponseHelper<string>.Error(
                        "Interviewer not found.",
                        statusCode: StatusCodeEnum.NOT_FOUND
                    )
                );
            }

            // 🔹 Fetch Retell API key
            var retellKey = await interviewerRepository.GetRetellAiKey(organizationId);
            if (string.IsNullOrEmpty(retellKey))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error(
                        "RetellAI API Key is not configured for this organization.",
                        statusCode: StatusCodeEnum.BAD_REQUEST
                    )
                );
            }

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", retellKey);
            http.DefaultRequestHeaders.Add("Accept", "application/json");

            string newAgentId = existing.Agent_id;
            string newVoiceId = existing.Voice_id;
            string newRecordUrl = existing.Record_url;
            string newName = model.Name;

            // 🔹 Voice change → create new agent
            if (!string.Equals(existing.Voice_id, model.Voice_id, StringComparison.OrdinalIgnoreCase))
            {
                newVoiceId = model.Voice_id;

                var requestPayload = new AgentCreateRequestViewModel
                {
                    ResponseEngine = new AgentData
                    {
                        LlmId = await interviewerRepository.Get_retell_LLM_key(organizationId),
                        Type = "retell-llm"
                    },
                    VoiceId = newVoiceId
                };

                var json = JsonConvert.SerializeObject(requestPayload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync(_retellUrls.CreateAgent, httpContent);
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                var agentResponse = JsonConvert.DeserializeObject<RetellCreateAgentResponse>(resultJson);

                if (agentResponse == null || string.IsNullOrEmpty(agentResponse.Agent_id))
                {
                    return new ObjectResult(
                        ResponseHelper<string>.Error(
                            "Failed to create Retell agent.",
                            statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                        )
                    );
                }

                newAgentId = agentResponse.Agent_id;

                var voiceData = await interviewerRepository.GetVoiceById(newVoiceId, retellKey);
                newRecordUrl = voiceData?.Preview_audio_url;
            }

            // 🔹 Agent name update
            if (!string.Equals(existing.Agent_id, model.Agent_id, StringComparison.OrdinalIgnoreCase))
            {
                newAgentId = model.Agent_id;

                var payload = new
                {
                    agent_name = newName,
                    pii_config = new
                    {
                        mode = "post_call",
                        categories = Array.Empty<string>()
                    }
                };

                var json = JsonConvert.SerializeObject(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var updateUrl = _retellUrls.UpdateAgent(newAgentId);
                var response = await http.PatchAsync(updateUrl, httpContent);
                response.EnsureSuccessStatusCode();

                var voiceData = await interviewerRepository.GetVoiceById(newVoiceId, retellKey);
                newRecordUrl = voiceData?.Preview_audio_url;
            }

            // 🔹 Avatar update
            string newAvatarFileName = existing.Avatar_url;

            if (model.Avatar != null)
            {
                _logger.LogInformation(
                    "New avatar uploaded for interviewer Id={Id}",
                    model.Id
                );

                string interviewerIdFolder = existing.Id.ToString();
                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    _uploadPath,
                    interviewerIdFolder
                );

                if (!string.IsNullOrWhiteSpace(existing.Avatar_url))
                {
                    string oldFilePath = Path.Combine(folderPath, existing.Avatar_url);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var ext = Path.GetExtension(model.Avatar.FileName);
                newAvatarFileName = $"{Guid.NewGuid()}{ext}";

                await SaveAvatarFile(model.Avatar, interviewerIdFolder, newAvatarFileName);
            }

            // 🔹 DB update
            var updateData = new InterviewerDataUpdateRequestViewModel
            {
                Id = model.Id.Value,
                Name = newName,
                Voice_id = newVoiceId,
                Agent_id = newAgentId,
                Avatar_url = newAvatarFileName,
                Record_url = newRecordUrl
            };

            var updated = await interviewerRepository.UpdateInterviewerAsync(updateData);
            if (updated == null)
            {
                return new ObjectResult(
                    ResponseHelper<string>.Error(
                        "Failed to update interviewer.",
                        statusCode: StatusCodeEnum.INTERNAL_SERVER_ERROR
                    )
                );
            }

            var updatedResponse = await interviewerRepository.GetInterviewerByIdAsync(model.Id.Value);
            if (!string.IsNullOrEmpty(updatedResponse?.Avatar_url))
            {
                updated.Avatar_url = GenerateImageUrl(
                    updated.Id.ToString(),
                    updatedResponse.Avatar_url
                );
            }

            _logger.LogInformation(
                "Interviewer updated successfully. Id={Id}",
                updated.Id
            );

            return new OkObjectResult(
                ResponseHelper<InterviewerCreateUpdateResponseViewModel>.Success(
                    "Interviewer updated successfully.",
                    updated
                )
            );
        }

        #endregion

        #region Save Avatar Async
        private async Task SaveAvatarFile(IFormFile avatar, string interviewerId, string fileName)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath, interviewerId);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }
        }

        #endregion
    }
}
