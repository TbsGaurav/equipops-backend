using Common.Services.Services.Implementation;
using Common.Services.Services.Interface;

using InterviewService.Api.Services.Implementation;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Helpers.EncryptionHelpers.Handlers;
using OrganizationService.Api.Infrastructure;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.Infrastructure.Repositories;
using OrganizationService.Api.Services.Implementation;
using OrganizationService.Api.Services.Interface;

namespace OrganizationService.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithRegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            services.AddScoped<IOrganizationService, Services.Implementation.OrganizationService>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<TokenHelper>();
            services.AddScoped<EncryptionHelper>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ICandidateService, CandidateService>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();

            services.AddScoped<IOrganizationSettingService, OrganizationSettingService>();
            services.AddScoped<IOrganizationSettingRepository, OrganizationSettingRepository>();

            services.AddScoped<IOrganizationLocationService, OrganizationLocationService>();
            services.AddScoped<IOrganizationLocationRepository, OrganizationLocationRepository>();

            services.AddScoped<IJobPostService, JobPostService>();
            services.AddScoped<IJobPostRepository, JobPostRepository>();

            services.AddScoped<IResumeParseService, ResumeParseService>();
            services.AddScoped<IMatchMachingService, MatchMachingService>();

            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();

            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            services.AddScoped<IGeneralAIService, GeneralAIService>();

            return services;
        }
    }
}
