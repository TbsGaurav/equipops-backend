
using Common.Services.Services.Implementation;
using Common.Services.ViewModels.RetellAI;

using InterviewService.Api.Helpers.EncryptionHelpers.Handlers;
using InterviewService.Api.Helpers.ResponseHelpers.Handlers;
using InterviewService.Api.Helpers.ResponseHelpers.Models;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Services.Interface;
using InterviewService.Api.ViewModels.Request.Interview;
using InterviewService.Api.ViewModels.Request.Interview_Que;
using InterviewService.Api.ViewModels.Response.Interview;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Net.Http.Headers;
using System.Text;

namespace InterviewService.Api.Services.Implementation
{
	public class InterviewService : IInterviewService
	{
		private readonly IInterviewRepository _interviewRepository;
		private readonly IInterviewerRepository _interviewerRepository;
		private readonly IInterviewQueRepository _interviewQueRepository;
		private readonly EncryptionHelper _encryptionHelper;
		private readonly RetellAIEndpoints _retellUrl;
		private readonly IConfiguration _configuration;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<InterviewService> _logger;
		private readonly string _uploadInterviewerPath;
		private readonly string _uploadInterviewPath;
		private readonly string _gatewayBaseUrl;

		public InterviewService(
			IInterviewRepository interviewRepository,
			 IConfiguration configuration,
			ILogger<InterviewService> logger,
			IHttpContextAccessor httpContextAccessor,
			EncryptionHelper encryptionHelper,
			 IInterviewQueRepository interviewQueRepository,
			 RetellAIEndpoints retellUrl,
			 IInterviewerRepository interviewerRepository
			)
		{
			_interviewRepository = interviewRepository;
			_configuration = configuration;
			_logger = logger;
			_uploadInterviewerPath = _configuration["ImageUploadSettings:InterviewerUploadFolderPath"] ?? "Uploads/Interviewer";
			_uploadInterviewPath = _configuration["ImageUploadSettings:InterviewUploadFolderPath"] ?? "Uploads/Interview";
			_interviewQueRepository = interviewQueRepository;
			_gatewayBaseUrl = _configuration["ImageUploadSettings:BaseUrl"]
							  ?? string.Empty;
			_httpContextAccessor = httpContextAccessor;
			_encryptionHelper = encryptionHelper;
			_retellUrl = retellUrl;
			_interviewerRepository = interviewerRepository;
		}

		public async Task<ApiResponse<InterviewListResponseViewModel>> GetInterviewsAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null, Guid? OrganizationId = null)
		{
			// 🔹 Repository Call
			_logger.LogInformation("Calling InterviewRepository.GetInterviewsAsync.");

			var user = _httpContextAccessor.HttpContext?.User;

			Guid? finalOrganizationId = null;
			if (OrganizationId.HasValue)
			{
				finalOrganizationId = OrganizationId.Value;
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

			var data = await _interviewRepository.GetInterviewsAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive, finalOrganizationId);

			if (data?.InterviewData != null && data.InterviewData.Any())
			{
				foreach (var item in data.InterviewData)
				{
					if (!string.IsNullOrWhiteSpace(item.Document))
					{
						item.Document = GenerateInterviewDocumentUrl(
							item.Id.ToString(),
							item.Document
						);
					}
				}
			}

			// 🔹 Success
			_logger.LogInformation("Interview retrieved successfully.");

			return new ApiResponse<InterviewListResponseViewModel>
			{
				Success = true,
				Message = "Interviews retrieved successfully.",
				Data = data
			};
		}

		public async Task<ApiResponse<InterviewByIdResponseViewModel>> GetInterviewByIdAsync(Guid Id)
		{
			// 🔹 Repository Call
			_logger.LogInformation("Calling InterviewRepository.GetInterviewByIdAsync.");

			var data = await _interviewRepository.GetInterviewByIdAsync(Id);

			if (data?.Interview != null && !string.IsNullOrWhiteSpace(data.Interview.Document))
			{
				data.Interview.Document = GenerateInterviewDocumentUrl(
					data.Interview.Id.ToString(),
					data.Interview.Document
				);
			}

			var questions = await _interviewQueRepository.GetInterviewQuestionsByInterviewIdAsync(Id);
			data.questions = questions;

			// 🔹 Success
			_logger.LogInformation("Interview retrieved successfully.");

			return new ApiResponse<InterviewByIdResponseViewModel>
			{
				Success = true,
				Message = "Interview retrieved successfully.",
				Data = data
			};
		}

		public async Task<ApiResponse<InterviewCreateResponseViewModel>> CreateInterviewAsync(InterviewCreateRequestViewModel model)
		{
			_logger.LogInformation("InterviewService: CreateInterviewAsync START. Name={Name}", model.Name);

			// 🔹 Validate Input
			if (string.IsNullOrEmpty(model.Name))
			{
				_logger.LogWarning("Validation failed: Required fields are missing. Name={Name}", model.Name);

				return new ApiResponse<InterviewCreateResponseViewModel>
				{
					Success = false,
					Message = "Interview Name is required."
				};
			}
			// 🔹 Repository Call
			_logger.LogInformation("Calling InterviewRepository.CreateInterviewAsync for Name={Name}", model.Name);

			string documentFileName = null;
			if (model.Document != null)
			{
				var ext = Path.GetExtension(model.Document.FileName);
				documentFileName = $"{Guid.NewGuid()}{ext}";
			}
			string documentFile = documentFileName;
			var data = await _interviewRepository.CreateInterviewAsync(model, documentFile);

			if (data == null || data.Id == Guid.Empty)
			{
				_logger.LogWarning("Interview creation failed. No Id returned. Name={Name}", model.Name);

				return new ApiResponse<InterviewCreateResponseViewModel>
				{
					Success = false,
					Message = "Interview creation failed.",
					Data = data
				};
			}
			if (documentFile != null)
			{
				string interviewFolder = data.Id.ToString();
				await SaveInterviewDocument(model.Document, interviewFolder, documentFileName);

				data.Document = GenerateInterviewDocumentUrl(interviewFolder, documentFileName);
			}


			// 🔹 Success
			_logger.LogInformation("Interview created successfully. Name={Name}", model.Name);

            return new ApiResponse<InterviewCreateResponseViewModel>
            {
                Success = true,
                Message = "Interview created successfully.",
                Data = data
            };
        }
        private async Task SaveInterviewDocument(IFormFile document, string interviewId, string fileName)
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                _uploadInterviewPath,
                interviewId
            );

			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await document.CopyToAsync(stream);
        }
        private string GenerateInterviewDocumentUrl(string interviewId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(interviewId) || string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

			var physicalPath = Path.Combine(
				Directory.GetCurrentDirectory(),
				_uploadInterviewPath,
				interviewId,
				fileName
			);

			if (!System.IO.File.Exists(physicalPath))
				return string.Empty;

			var publicPath =
				$"interview/uploads/Interview/{interviewId}/{fileName}"
				.Replace("\\", "/");

            return $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";
        }
        public async Task<ApiResponse<InterviewListResponseViewModel>> DeleteInterviewAsync(InterviewDeleteRequestViewModel model)
        {
            // 🔹 Repository Call
            _logger.LogInformation("InterviewService: DeleteInterviewAsync START. with Id={Id}", model.Id);

			await _interviewRepository.DeleteInterviewAsync(model);

			// 🔹 Fetch Updated List
			var data = await _interviewRepository.GetInterviewsAsync(
				Search: null,
				Length: 10,
				Page: 1,
				OrderColumn: "name",
				OrderDirection: "Asc",
				IsActive: null,
				OrganizationId: null
			);

			if (data?.InterviewData != null && data.InterviewData.Any())
			{
				foreach (var item in data.InterviewData)
				{
					if (!string.IsNullOrWhiteSpace(item.Document))
					{
						item.Document = GenerateInterviewDocumentUrl(
							item.Id.ToString(),
							item.Document
						);
					}
				}
			}

			// 🔹 Success
			_logger.LogInformation("Interview deleted successfully. Id={Id}", model.Id);

            return new ApiResponse<InterviewListResponseViewModel>
            {
                Success = true,
                Message = "Interview deleted successfully.",
                Data = data
            };
        }
        public async Task<IActionResult> GetInterviewInitAsync()
        {
            _logger.LogInformation("Calling InterviewService.GetInterviewInitAsync.");

			var data = await _interviewRepository.GetInterviewInitAsync();

			// 🔹 Generate avatar URLs
			if (data?.InterviewerList != null && data.InterviewerList.Any())
			{
				foreach (var item in data.InterviewerList)
				{
					if (!string.IsNullOrWhiteSpace(item.Avatar_url))
					{
						item.Avatar_url = GenerateImageUrl($"{item.Id}", item.Avatar_url);
					}
				}
			}

			_logger.LogInformation("Interview init data retrieved successfully.");

            return new OkObjectResult(
                ResponseHelper<InterviewInitResponseViewModel>.Success(
                    "Interview init data retrieved successfully.",
                    data
                )
            );
        }
        private string GenerateImageUrl(string folderName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

			var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadInterviewerPath, folderName, fileName);
			if (!System.IO.File.Exists(physicalPath))
				return string.Empty;

			var publicPath = $"interview/uploads/Interviewer/{folderName}/{fileName}".Replace("\\", "/");

            return $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";
        }
        public async Task<ApiResponse<InterviewUpdateResponseViewModel>> UpdateInterviewAsync(InterviewUpdateRequestViewModel model)
        {
            _logger.LogInformation("InterviewService: UpdateInterviewAsync START. Name={Name}", model.Name);

			// 🔹 Validate Input
			if (string.IsNullOrEmpty(model.Name))
			{
				return new ApiResponse<InterviewUpdateResponseViewModel>
				{
					Success = false,
					Message = "Interview Name is required."
				};
			}
			// 🔹 Get existing interview
			var interviewResponse = await _interviewRepository.GetInterviewByIdAsync(model.Id ?? Guid.Empty);
			var existingDocument = interviewResponse?.Interview?.Document;
			string documentFileName = existingDocument;

			// 🔹 Handle document upload (if any)
			if (model.Document != null)
			{
				_logger.LogInformation("New document uploaded for Interview Id={Id}", model.Id);

				string interviewIdFolder = model.Id.ToString();
				string folderPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadInterviewerPath, interviewIdFolder);

				// Delete old file
				if (!string.IsNullOrWhiteSpace(existingDocument))
				{
					string oldFilePath = Path.Combine(folderPath, existingDocument);
					if (System.IO.File.Exists(oldFilePath))
					{
						System.IO.File.Delete(oldFilePath);
					}
				}

				var ext = Path.GetExtension(model.Document.FileName);
				documentFileName = $"{Guid.NewGuid()}{ext}";

				await SaveInterviewDocument(model.Document, interviewIdFolder, documentFileName);
			}

			// 🔹 Update interview in repository (with or without new document)
			var updatedInterview = await _interviewRepository.UpdateInterviewAsync(model, documentFileName);

			if (updatedInterview == null)
			{
				return new ApiResponse<InterviewUpdateResponseViewModel>
				{
					Success = false,
					Message = "Interview update failed."
				};
			}

			// 🔹 Bulk create/update questions
			if (model.Questions != null && model.Questions.Any())
			{
				var questionRequest = new InterviewQueRequestViewModel
				{
					interview_id = updatedInterview.Id.Value,
					questions = model.Questions
				};

				await _interviewQueRepository.InterviewQueCreateAsync(questionRequest);
			}

            // 🔹 Generate public document URL
            if (!string.IsNullOrWhiteSpace(documentFileName))
            {
                updatedInterview.Document = GenerateInterviewDocumentUrl(
                    updatedInterview.Id.Value.ToString(),
                    documentFileName
                );
            }
            var questions = await _interviewQueRepository.GetInterviewQuestionsByInterviewIdAsync(updatedInterview.Id.Value);
            updatedInterview.Questions = questions;
            return new ApiResponse<InterviewUpdateResponseViewModel>
            {
                Success = true,
                Message = "Interview updated successfully.",
                Data = updatedInterview
            };
        }
        public async Task<ApiResponse<string>> CreateInterviewTokenAsync(InterviewTokenRequestViewModel request)
        {
            var data = await _interviewRepository.CreateInterviewTokenAsync(request);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Interview token retrieved successfully.",
                Data = data
            };
        }
        public async Task<IActionResult> VerifyInterviewTokenAsync(VerifyTokenRequestViewModel model)
        {
            _logger.LogInformation("VerifyInterviewTokenAsync START");

            if (string.IsNullOrWhiteSpace(model.Token))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error("Token is required.")
                );
            }
            var (statusCode, repoMessage, tokenData) =
                await _interviewRepository.VerifyInterviewTokenAsync(model.Token);

            if (statusCode != 200)
            {
                return new ObjectResult(new
                {
                    success = false,
                    message = repoMessage
                })
                {
                    StatusCode = statusCode
                };
            }
            // 🔹 Deserialize token payload
            if (string.IsNullOrWhiteSpace(tokenData))
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error("Invalid token data.")
                );
            }
            var payload = JsonConvert.DeserializeObject<InterviewTokenResponseViewModel>(tokenData);
            if (payload == null)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error("Invalid token payload.")
                );
            }
            // 🔹 Fetch interview invitation
            var interviewInvitation =await _interviewRepository.GetCandidateInterviewInvitationAsync(payload.Candidate_interview_invitation_id);

            if (interviewInvitation == null || interviewInvitation.InterviewId == Guid.Empty)
            {
                return new BadRequestObjectResult(
                    ResponseHelper<string>.Error("Interview invitation not found.")
                );
            }
            // 🔹 Timezone handling
            var timeZoneId =
                _httpContextAccessor.HttpContext?.Request.Headers["X-Timezone"]
                    .FirstOrDefault();
            DateTime interviewUtc = payload.Interview_date; 
            DateTime interviewLocal = interviewUtc;

            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                    interviewLocal = TimeZoneInfo.ConvertTimeFromUtc(interviewUtc, tz);
                }
                catch
                {
                    interviewLocal = interviewUtc;
                }
            }

            // 🔹 Remaining minutes calculation 
            int remainingMinutes = 0;
            var nowUtc = DateTime.UtcNow;

            if (nowUtc < interviewUtc)
            {
                remainingMinutes = (int)Math.Floor(
                    (interviewUtc - nowUtc).TotalMinutes
                );
            }
            // 🔹 Decide message (API is source of truth)
            string responseMessage = remainingMinutes > 0
                ? "You have pending time to start interview."
                : "Interview token verified successfully.";

            // 🔹 Fetch interview overview
            var interviewData =
                await _interviewRepository.GetInterviewByIdAsync(
                    interviewInvitation.InterviewId
                );

            Guid interviewerId = interviewData.Interview.Interviewer_Id;
            Guid organizationId = interviewData.Interview.Organization_Id;

            var interviewOverview =
                await _interviewRepository.GetInterviewOverviewAsync(
                    interviewInvitation.InterviewId,
                    interviewInvitation.CandidateId,
                    interviewerId
                );

            // 🔹 Resolve avatars
            if (!string.IsNullOrEmpty(interviewOverview?.Interviewer?.Interviewer_avatar))
            {
                interviewOverview.Interviewer.Interviewer_avatar =
                    GenerateImageUrl(
                        interviewerId.ToString(),
                        interviewOverview.Interviewer.Interviewer_avatar
                    );
            }
            if (!string.IsNullOrEmpty(interviewOverview?.Candidate?.Candidate_avatar))
            {
                var generator = new GenerateCandidateImageUrl();
                interviewOverview.Candidate.Candidate_avatar =
                    generator.GenerateImageUrl(
                        interviewInvitation.CandidateId.ToString(),
                        interviewOverview.Candidate.Candidate_avatar,
                        _gatewayBaseUrl
                    );
            }
            // 🔹 Final response
            return new OkObjectResult(new
            {
                status = 1,
                message = responseMessage,
                data = new
                {
                    interviewInvitation.InterviewId,
                    interviewerId,
                    interviewInvitation.OrganizationId,
                    interviewInvitation.CandidateId,
                    candidateData = interviewOverview.Candidate,
                    interviewerData = interviewOverview.Interviewer,
                    interviewData = interviewOverview.Interview,
                    remainingMinutes,
                    interviewDateLocal = interviewLocal
                }
            });
        }
        public async Task<IActionResult> RegisterCallAsync(CallRegisterRequestViewModel model)
        {
            try
            {
                _logger.LogInformation("VerifyInterviewTokenAsync START");

				if (string.IsNullOrWhiteSpace(model.Token))
				{
					return new BadRequestObjectResult(new
					{
						status = 0,
						message = "Token is required.",
						data = (object?)null
					});
				}

				if (string.IsNullOrWhiteSpace(model.Email))
				{
					return new BadRequestObjectResult(new
					{
						status = 0,
						message = "Email is required.",
						data = (object?)null
					});
				}
				//  Decrypt token
				var decryptedJson = _encryptionHelper.DecryptFromReact(model.Token);

                // Deserialize payload
                var payload = JsonConvert.DeserializeObject<InterviewTokenResponseViewModel>(decryptedJson);
                var InterviewInvitation= await _interviewRepository.GetCandidateInterviewInvitationAsync(payload.Candidate_interview_invitation_id);

                var candidateEmail = await _interviewRepository.GetCandidateEmailByIdAsync(InterviewInvitation.CandidateId);

				if (string.IsNullOrWhiteSpace(candidateEmail))
				{
					return new BadRequestObjectResult(new
					{
						status = 0,
						message = "Candidate not found.",
						data = (object?)null
					});
				}

                // 4. Compare email from token candidate with request email
                if (!string.Equals(candidateEmail.Trim(), model.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return new BadRequestObjectResult(new
                    {
                        status = 0,
                        message = "Email does not match the candidate.",
                        data = (object?)null
                    });
                }
                //fetch interview data
                var interviewData = await _interviewRepository.GetInterviewByIdAsync(InterviewInvitation.InterviewId);
                var interviewerId = interviewData.Interview.Interviewer_Id;

				if (interviewerId == Guid.Empty)
				{
					return new BadRequestObjectResult(new
					{
						status = 0,
						message = "Interviewer Id is required.",
						data = (object?)null
					});
				}


				Guid organizationId = interviewData.Interview.Organization_Id;

				if (organizationId == Guid.Empty)
				{
					return new BadRequestObjectResult(new
					{
						status = 0,
						message = "Organization Id is required.",
						data = (object?)null
					});
				}
				//fetch agent Id 
				var interview = await _interviewerRepository.GetInterviewerByIdAsync(interviewerId);

                var questionsData = await _interviewQueRepository.GetInterviewQuestionsByInterviewIdAsync(InterviewInvitation.InterviewId)?? new List<InterviewQuestionBulkDto>();
                var questions = questionsData
                    .Select(q => q.Question)
                    .Where(q => !string.IsNullOrWhiteSpace(q))
                    .ToList();
                _logger.LogInformation(
                    "First Question Value: {Question}",
                    questionsData.FirstOrDefault()?.Question
                );

                var retellApiKey = await _interviewerRepository.GetRetellAiKey(organizationId);
                var llmId = await _interviewerRepository.Get_retell_LLM_key(organizationId);

                var retellRequest = new RetellCallRequest
                {
                    AgentId = interview.Agent_id,
                    AgentOverride = new AgentOverride
                    {
                        Agent = new Agent
                        {
                            Language = "multi"
                        }
                    },
                    DynamicVariables = new Dictionary<string, string>
                    {
                        { "mins", interviewData.Interview.Duration_Mins.ToString() },
                        { "objective", interviewData.Interview.Description },
                        { "questions", string.Join("\n- ", questions) },
                        { "name", model.Name ?? "" }
                    }
                };

                using var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", retellApiKey);
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                var json = JsonConvert.SerializeObject(retellRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync(_retellUrl.CreateWebCall, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Retell API error: {error}", error);
                    return new BadRequestObjectResult(new
                    {
                        status = 0,
                        message = $"Failed: {error}",
                        data = (object?)null
                    });
                }

                response.EnsureSuccessStatusCode();
                var retellResponse = await response.Content.ReadFromJsonAsync<RetellCallRegisterResponseViewModel>();

                var callRegister = await _interviewRepository.UpdateCallIdAsync(
                    payload.Candidate_interview_invitation_id,
                    retellResponse.Call_id
                );
                // Return Retell response directly
                return new OkObjectResult(new
                {
                    status = 1,
                    message = "Call registered successfully.",
                    data = retellResponse
                });
            }

			catch (Exception ex)
			{
				_logger.LogError(ex, "RegisterCallAsync failed");

				return new BadRequestObjectResult(new
				{
					status = 0,
					message = "Failed to register call.",
					data = (object?)null
				});
			}

        }
    }
}
