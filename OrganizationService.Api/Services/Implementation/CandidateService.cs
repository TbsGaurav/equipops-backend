using Common.Services.Services.Interface;
using Common.Services.ViewModels.MatchMatching;
using Common.Services.ViewModels.ResumeParse;

using DocumentFormat.OpenXml.Packaging;

using Microsoft.AspNetCore.Mvc;

using OrganizationService.Api.Helpers.ResponseHelpers.Enums;
using OrganizationService.Api.Helpers.ResponseHelpers.Handlers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Services.Interface;
using OrganizationService.Api.ViewModels.Request.Candidate;
using OrganizationService.Api.ViewModels.Response.Candidate;

using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace OrganizationService.Api.Services.Implementation
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<CandidateService> _logger;
        private readonly string _uploadResumePath;
        private readonly string _gatewayBaseUrl;
        private readonly IConfiguration _configuration;
        private readonly IResumeParseService _resumeParseService;
        private readonly IMatchMachingService _matchMachingService;
        private readonly IEmailService _emailService;

        public CandidateService(
            ICandidateRepository candidateRepository,
            ILogger<CandidateService> logger,
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor,
            IResumeParseService resumeParseService,
            IMatchMachingService matchMachingService, IEmailService emailService
            )
        {
            _candidateRepository = candidateRepository;
            _logger = logger;
            _configuration = configuration;
            _uploadResumePath = _configuration["ResumeUploadSettings:ResumeUploadFolderPath"] ?? "Uploads/Resume";
            _gatewayBaseUrl = _configuration["ResumeUploadSettings:BaseUrl"] ?? string.Empty;
            _contextAccessor = contextAccessor;
            _resumeParseService = resumeParseService;
            _matchMachingService = matchMachingService;
            _emailService = emailService;
        }

        public async Task<IActionResult> GetCandidatesAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            // 🔹 Repository Call
            _logger.LogInformation("Calling CandidateRepository.GetCandidatesAsync.");

            var data = await _candidateRepository.GetCandidatesAsync(Search, Length, Page, OrderColumn, OrderDirection, IsActive);

            // 🔹 Failure
            if (data == null)
            {
                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                    "Candidates not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            _logger.LogInformation("Candidate retrieved successfully.");

            return new OkObjectResult(ResponseHelper<CandidateListResponseViewModel>.Success(
                "Candidates retrieved successfully.",
                data
            ));
        }

        public async Task<IActionResult> GetCandidateByIdAsync(Guid Id)
        {
            // 🔹 Validate Input
            if (Id == Guid.Empty)
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Id={Id}", Id);

                return new BadRequestObjectResult(ResponseHelper<CandidateByIdResponseViewModel>.Error(
                    "Id is required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Repository Call
            _logger.LogInformation("Calling CandidateRepository.GetCandidateByIdAsync.");

            var data = await _candidateRepository.GetCandidateByIdAsync(Id);

            // 🔹 Failure
            if (data == null)
            {
                return new NotFoundObjectResult(ResponseHelper<string>.Error(
                    "Candidate not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            _logger.LogInformation("Candidate retrieved successfully.");

            return new OkObjectResult(ResponseHelper<CandidateByIdResponseViewModel>.Success(
                "Candidate retrieved successfully.",
                data
            ));
        }

        public async Task<IActionResult> CreateUpdateCandidateAsync(CandidateCreateUpdateRequestViewModel model, bool IsDirect)
        {
            // 🔹 Validate Input
            if (model.json_form_data.InterviewId == Guid.Empty || string.IsNullOrEmpty(model.json_form_data.Name))
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Name={Name}", model.json_form_data.Name);

                return new BadRequestObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Error(
                    "Candidate Name and InterviewId are required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Repository Call
            string Resume_url = "";
            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), _uploadResumePath, "[Candidate_Id]");
            var safeFileName = Path.GetFileName(model.json_form_data.Resume.FileName);
            var fileName = $"[Candidate_Id]_{safeFileName}";
            var fullPath = Path.Combine(uploadRoot, fileName);

            Resume_url = $"Uploads/Resume/[Candidate_Id]/{fileName}";
            if (model.json_form_data.Resume == null && model.json_form_data.Resume.Length == 0)
            {
                Resume_url = "";
            }

            var ParseResumeResponse = await ParseResumeAsync(model.json_form_data.Resume, model.json_form_data.InterviewId);
            //var data1 = ((ResponseResult<MatchMatchingResponse>)((ObjectResult)ParseResumeResponse).Value).Data.parseResponse;
            if (ParseResumeResponse != null)
            {
                string TotalExperience = ParseResumeResponse.parseResponse.TotalExperienceYears.ToString(); // candidate total experiance from resume
                //string TotalExperience = "4";
                string Skill = string.Join(", ", ParseResumeResponse.parseResponse.Skills); // candidate skills from resume
                //string Skill = "Fiix, Megger Insulation Tester, Siemens STEP 7, Ignition by Inductive Automation, OrCAD, Schneider Electric EcoStruxure, SKF Microlog Analyzer, FLIR Thermal Cameras, PRUFTECHNIK, Maximo";
                if (model.json_form_data.Phone_Number == null)
                {
                    model.json_form_data.Phone_Number = ParseResumeResponse.parseResponse.Phone; // candidate phone number from resume
                }

                dynamic Form_Data = new ExpandoObject();
                JsonElement json;

                Form_Data.json_form_data = model.json_form_data;
                Form_Data.ParseResumeJson = ParseResumeResponse;

                using (var doc = JsonDocument.Parse(JsonSerializer.Serialize(Form_Data)))
                {
                    json = doc.RootElement.Clone();
                }

                // saving candidate details
                var data = await _candidateRepository.CreateUpdateCandidateAsync(model, Resume_url, TotalExperience, Skill, json, Convert.ToDecimal(ParseResumeResponse.matchScore));

                /* -------------------- DELETE OLD Resume -------------------- */
                //var Resume = await _organizationRepository.GetExistingProfileResumeAsync(model.Id);

                //if (!string.IsNullOrWhiteSpace(existingResume))
                //{
                //    var oldFilePath = Path.Combine(uploadRoot, Path.GetFileName(existingResume));

                //    if (File.Exists(oldFilePath))
                //    {
                //        File.Delete(oldFilePath);
                //    }
                //    else
                //    {
                //    }
                //}

                if (model.json_form_data.Resume != null && model.json_form_data.Resume.Length > 0)
                {
                    uploadRoot = uploadRoot.Replace("[Candidate_Id]", data.Id.ToString());
                    fileName = fileName.Replace("[Candidate_Id]", data.Id.ToString());
                    fullPath = Path.Combine(uploadRoot, fileName);

                    if (!Directory.Exists(uploadRoot))
                        Directory.CreateDirectory(uploadRoot);

                    /* -------------------- SAVE NEW Resume -------------------- */
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.json_form_data.Resume.CopyToAsync(stream);
                    }
                }

                //float total_score = ((ResponseResult<MatchMatchingResponse>)((ObjectResult)ParseResumeResponse).Value).Data.matchScore;
                float total_score = ParseResumeResponse.matchScore;
                //float total_score = 84;

                if (IsDirect || total_score >50)
                {
                    InterviewTokenRequestViewModel request = new InterviewTokenRequestViewModel();
                    request.Candidate_id = data.Id;
                    request.Interview_id = data.InterviewId;
                    request.Interview_date = DateTime.UtcNow;
                    // interview invitation link
                    await CreateInterviewTokenAsync(request, model.json_form_data.Email, IsDirect);
                }
                data.TotalScore = total_score;
                data.TotalExperience = TotalExperience;
                var publicPath = $"org/{Resume_url}".Replace("\\", "/");
                var fullUrl = $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";
                data.ResumeUrl = fullUrl;

                // 🔹 Failure
                if (data == null || data.Id == Guid.Empty)
                {
                    return new ConflictObjectResult(ResponseHelper<string>.Error(
                        "Candidate creation/updation failed.",
                        statusCode: StatusCodeEnum.CONFLICT_OCCURS
                    ));
                }

                // 🔹 Success
                if (model.json_form_data.Id != null)
                {
                    return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
                        "Candidate updated successfully.",
                        data
                    ));
                }
                return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
                    "Candidate created successfully.",
                    data
                ));
            }
            else
            {
                return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
                    "Something went wrong please contact adminstration",
                    null
                ));
            }
        }

        public async Task<IActionResult> CanidateResumeBulkAsync(List<IFormFile> model)
        {
            // 🔹 Repository Call
            if (model != null && model.Count > 0) 
            {

                var ParseResumeResponse = await ParseResumeAsyncBatch(model, Guid.Empty);
                //foreach (var resume in model) {
                //    string Resume_url = "";
                //    if (resume != null && resume.Length > 0)
                //    {
                //        var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), _uploadResumePath, "");

                //        if (!Directory.Exists(uploadRoot))
                //        {
                //            Directory.CreateDirectory(uploadRoot);
                //        }

                //        /* -------------------- SAVE NEW Resume -------------------- */
                //        var safeFileName = Path.GetFileName(resume.FileName);
                //        var fileName = $"{safeFileName}";
                //        var fullPath = Path.Combine(uploadRoot, fileName);

                //        using (var stream = new FileStream(fullPath, FileMode.Create))
                //        {
                //            await resume.CopyToAsync(stream);
                //        }
                //        Resume_url = $"Uploads/Resume//{fileName}";
                //    }
                    
                //}
            }
            //if (model.ResumeDetail != null && model.ResumeDetail.Count > 0)
            //{
            //    _logger.LogInformation("Resume upload detected | Size={Size}", model.ResumeDetail.Count);

            //    var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), _uploadResumePath, "");

            //    _logger.LogDebug("Resolved upload directory | Path={UploadRoot}", uploadRoot);

            //    if (!Directory.Exists(uploadRoot))
            //    {
            //        Directory.CreateDirectory(uploadRoot);
            //        _logger.LogInformation("Upload directory created | Path={UploadRoot}", uploadRoot);
            //    }

            //    /* -------------------- SAVE NEW Resume -------------------- */
            //    var safeFileName = Path.GetFileName(model.ResumeDetail[0].Resume.FileName);
            //    var fileName = $"{model.json_form_data.Id}_{safeFileName}";
            //    var fullPath = Path.Combine(uploadRoot, fileName);

            //    using (var stream = new FileStream(fullPath, FileMode.Create))
            //    {
            //        await model.ResumeDetail[0].Resume.CopyToAsync(stream);
            //    }

            //    Resume_url = $"Uploads/Resume/{fileName}";

            //    _logger.LogInformation("New profile Resume saved | FilePath={ResumePath}", Resume_url);
            //}

            //var ParseResumeResponse = await ParseResumeAsync(model.ResumeDetail[0], model.json_form_data.InterviewId);
            ////var data1 = ((ResponseResult<MatchMatchingResponse>)((ObjectResult)ParseResumeResponse).Value).Data.parseResponse;
            //if (ParseResumeResponse != null)
            //{
            //    string TotalExperience = ParseResumeResponse.parseResponse.ExperienceYears.ToString(); // candidate total experiance from resume
            //                                                                                           //string TotalExperience = "4";

            //    string Skill = string.Join(", ", ParseResumeResponse.parseResponse.Skills); // candidate skills from resume
            //                                                                                //string Skill = "Fiix, Megger Insulation Tester, Siemens STEP 7, Ignition by Inductive Automation, OrCAD, Schneider Electric EcoStruxure, SKF Microlog Analyzer, FLIR Thermal Cameras, PRUFTECHNIK, Maximo";

            //    if (model.json_form_data.Phone_Number == null)
            //    {
            //        model.json_form_data.Phone_Number = ParseResumeResponse.parseResponse.Phone; // candidate phone number from resume
            //    }

            //    // saving candidate details
            //    var data = await _candidateRepository.CreateUpdateCandidateAsync(model, Resume_url, TotalExperience, Skill);


            //    //float total_score = ((ResponseResult<MatchMatchingResponse>)((ObjectResult)ParseResumeResponse).Value).Data.matchScore;
            //    float total_score = ParseResumeResponse.matchScore;
            //    //float total_score = 84;

            //    //if (IsDirect || total_score > 50)
            //    //{
            //    //    InterviewTokenRequestViewModel request = new InterviewTokenRequestViewModel();
            //    //    request.Candidate_id = data.Id;
            //    //    request.Interview_id = data.InterviewId;
            //    //    request.Interview_date = DateTime.Now;
            //    //    // interview invitation link
            //    //    await CreateInterviewTokenAsync(request, model.json_form_data.Email);
            //    //}
            //    data.TotalScore = total_score;
            //    data.TotalExperience = TotalExperience;
            //    var publicPath = $"org/{Resume_url}".Replace("\\", "/");
            //    var fullUrl = $"{_gatewayBaseUrl.TrimEnd('/')}/{publicPath}";
            //    data.ResumeUrl = fullUrl;

            //    // 🔹 Failure
            //    if (data == null || data.Id == Guid.Empty)
            //    {
            //        return new ConflictObjectResult(ResponseHelper<string>.Error(
            //            "Candidate creation/updation failed.",
            //            statusCode: StatusCodeEnum.CONFLICT_OCCURS
            //        ));
            //    }

            //    // 🔹 Success
            //    if (model.json_form_data.Id != null)
            //    {
            //        return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
            //            "Candidate updated successfully.",
            //            data
            //        ));
            //    }
            //    return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
            //        "Candidate created successfully.",
            //        data
            //    ));
            //}
            //else
            //{
            //    return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
            //        "Something went wrong please contact adminstration",
            //        null
            //    ));
            //}

            return new OkObjectResult(ResponseHelper<CandidateCreateUpdateResponseViewModel>.Success(
                    "Something went wrong please contact adminstration",
                    null
                ));

        }

        public async Task<IActionResult> DeleteCandidateAsync(CandidateDeleteRequestViewModel model)
        {
            // 🔹 Validate Input
            if (model.Id == Guid.Empty)
            {
                _logger.LogWarning("Validation failed: Required fields are missing. Id={Id}", model.Id);

                return new BadRequestObjectResult(ResponseHelper<CandidateListResponseViewModel>.Error(
                    "Id is required",
                    statusCode: StatusCodeEnum.BAD_REQUEST
                ));
            }

            // 🔹 Repository Call
            _logger.LogInformation("CandidateService: DeleteCandidateAsync START. with Id={Id}", model.Id);

            await _candidateRepository.DeleteCandidateAsync(model);

            // 🔹 Fetch Updated List
            var data = await _candidateRepository.GetCandidatesAsync(
                Search: null,
                Length: 10,
                Page: 1,
                OrderColumn: "name",
                OrderDirection: "Asc"
            );

            // 🔹 Failure
            if (data == null)
            {
                return new NotFoundObjectResult(ResponseHelper<CandidateListResponseViewModel>.Error(
                    "Candidate not found.",
                    statusCode: StatusCodeEnum.NOT_FOUND
                ));
            }

            // 🔹 Success
            _logger.LogInformation("Candidate deleted successfully. Id={Id}", model.Id);

            return new OkObjectResult(ResponseHelper<CandidateListResponseViewModel>.Success(
                "Candidate deleted successfully.",
                data
            ));
        }


        #region Resume Parse
        public async Task<MatchMatchingResponse> ParseResumeAsync(IFormFile file, Guid interviewId)
        {
            if (file == null || file.Length == 0)
            {
                return new MatchMatchingResponse();
            }

            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream);

            // 🔹 Extract text from resume
            string resumeText;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            resumeText = extension switch
            {
                ".pdf" => ReadPdf(filePath),
                ".docx" => ReadDocx(filePath),
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(resumeText))
            {
                return new MatchMatchingResponse();
            }

            // 🔹 Get organization id
            var orgIdClaim = _contextAccessor.HttpContext?
                .User?
                .FindFirst("organization_id")?
                .Value;

            if (!Guid.TryParse(orgIdClaim, out var organizationId))
            {
                //return new BadRequestObjectResult(ResponseHelper<string>.Error("Invalid organization context", statusCode: StatusCodeEnum.BAD_REQUEST));
            }

            // 🔹 Get OpenAI Key
            var openAiKey = await _candidateRepository.GetOpenAiKey(organizationId, interviewId);

            if (string.IsNullOrWhiteSpace(openAiKey))
            {
                return new MatchMatchingResponse();
            }

            // 🔹 Parse resume using OpenAI
            var parsedResume = await _resumeParseService.ParseResumeAsync(resumeText, openAiKey);

            var skills = await _candidateRepository.GetSkillsAsync(interviewId);

            var job = new JobRequirementViewModel
            {
                RequiredSkills = skills ?? [],

                // need to do dynamic
                MinExperienceYears = 2
                //RequiredEducation = "Computer",
                //SalaryBudget = 700000
            };

            var matchResult = _matchMachingService.CalculateMatchScore(job, parsedResume.Profile);

            MatchMatchingResponse matchingResponse = new()
            {
                matchScore = matchResult,
                parseResponse = parsedResume.Profile,
                recommendation = GetRecommendation(matchResult),
            };

            return matchingResponse;
        }

        public async Task<List<MatchMatchingResponse>> ParseResumeAsyncBatch(List<IFormFile> files, Guid interviewId)
        {
            if (files == null || files.Count == 0)
                return new List<MatchMatchingResponse>();

            // 🔹 Get organization id from claims
            var orgIdClaim = _contextAccessor.HttpContext?
                .User?
                .FindFirst("organization_id")?
                .Value;

            if (!Guid.TryParse(orgIdClaim, out var organizationId))
                throw new InvalidOperationException("Invalid organization context");

            // 🔹 Get OpenAI Key
            var openAiKey = await _candidateRepository.GetOpenAiKey(organizationId, interviewId);

            if (string.IsNullOrWhiteSpace(openAiKey))
                throw new InvalidOperationException("OpenAI key not configured");

            var resumeTexts = new List<string>();

            // 🔹 Process each resume file
            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                var tempFilePath = Path.GetTempFileName();

                try
                {
                    await using (var stream = System.IO.File.Create(tempFilePath))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var extension = Path
                        .GetExtension(file.FileName)
                        .ToLowerInvariant();

                    string resumeText = extension switch
                    {
                        ".pdf" => ReadPdf(tempFilePath),
                        ".docx" => ReadDocx(tempFilePath),
                        _ => string.Empty
                    };

                    if (!string.IsNullOrWhiteSpace(resumeText))
                        resumeTexts.Add(resumeText);
                }
                finally
                {
                    // 🔹 Always clean temp file
                    if (System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                }
            }

            if (resumeTexts.Count == 0)
                return [];

            // 🔹 Parse resumes (batch)
            var parsedResumes = await _resumeParseService
                .ParseResumesBatchAsync(resumeTexts, openAiKey);

            // 🔹 Get job skills
            var skills = await _candidateRepository.GetSkillsAsync(interviewId);

            var job = new JobRequirementViewModel
            {
                RequiredSkills = skills ?? [],
                MinExperienceYears = 2 // TODO: make dynamic
            };

            List<MatchMatchingResponse> matchMatchingResponses = [];

            foreach (var item in parsedResumes)
            {
                // 🔹 Calculate match score
                var matchScore = _matchMachingService
                    .CalculateMatchScore(job, item.Profile);

                matchMatchingResponses.Add(new MatchMatchingResponse()
                {
                    matchScore = matchScore,
                    parseResponse = item.Profile,
                    recommendation = GetRecommendation(matchScore)
                });
            }

            return matchMatchingResponses;
        }
        #endregion

        #region Private Method
        private static string ReadPdf(string filePath)
        {
            var text = new StringBuilder();

            using var pdf = UglyToad.PdfPig.PdfDocument.Open(filePath);
            foreach (var page in pdf.GetPages())
            {
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }
        private static string ReadDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;
            return body.InnerText;
        }
        private static string GetRecommendation(float score)
        {
            if (score >= 80) return "Strong Match";
            if (score >= 60) return "Moderate Match";
            return "Weak Match";
        }

        #endregion

        #region Invitation email send
        public async Task<string> CreateInterviewTokenAsync(InterviewTokenRequestViewModel request, string Email, bool IsDirect)
        {
            var data = await _candidateRepository.CreateInterviewTokenAsync(request, IsDirect);
            var origin = _contextAccessor.HttpContext?.Request.Headers["Origin"].ToString();

            await _emailService.SendInterviewLinkAsync(Email, data);
            return data;
        }
        #endregion
    }
}
