using Common.Services.Services.Implementation;
using Common.Services.Services.Interface;

using InterviewService.Api.Helpers.EncryptionHelpers.Handlers;
using InterviewService.Api.Infrastructure;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.Infrastructure.Repositories;
using InterviewService.Api.Services.Implementation;
using InterviewService.Api.Services.Interface;

using SettingService.Api.Helpers;

namespace InterViewService.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithRegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            services.AddScoped<EncryptionHelper>();

            services.AddScoped<IInterviewService, InterviewService.Api.Services.Implementation.InterviewService>();
            services.AddScoped<IInterviewRepository, InterviewRepository>();

            services.AddScoped<IInterviewTypeService, InterviewTypeService>();
            services.AddScoped<IInterviewTypeRepository, InterviewTypeRepository>();

            services.AddScoped<IInterviewerService, InterviewerService>();
            services.AddScoped<IInterviewerRepository, InterviewerRepository>();

            services.AddScoped<IInterviewQueService, InterviewQueService>();
            services.AddScoped<IInterviewQueRepository, InterviewQueRepository>();

            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
            services.AddScoped<IInterviewerService, InterviewerService>();
            services.AddScoped<IInterviewerRepository, InterviewerRepository>();

            services.AddScoped<IInterviewerSettingService, InterviewerSettingService>();
            services.AddScoped<IInterviewerSettingRepository, InterviewerSettingRepository>();

            services.AddScoped<IInterviewDetailService, InterviewDetailService>();
            services.AddScoped<IInterviewDetailRepository, InterviewDetailRepository>();

            services.AddScoped<IInterviewTranscriptService, InterviewTranscriptService>();
            services.AddScoped<IInterviewTranscriptRepository, InterviewTranscriptRepository>();

            services.AddScoped<IInterviewFormService, InterviewFormService>();
            services.AddScoped<IInterviewFormRepository, InterviewFormRepository>();

            services.AddScoped<IInterviewEvaluationService, InterviewEvaluationService>();

            services.AddScoped<IGeneralAIService, GeneralAIService>();

            services.AddScoped<IJobAnalysisRepository, JobAnalysisRepository>();
            services.AddScoped<IJobAnalysisService, JobAnalysisService>();

            services.AddScoped<IWebhookService, WebhookService>();
            services.AddScoped<IWebHookRepository, WebHookRepository>();

            services.AddScoped<PgHelper>();

            return services;
        }
    }
}
