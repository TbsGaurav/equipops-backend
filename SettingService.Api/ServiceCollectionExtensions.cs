using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.Infrastructure.Repositories;
using SettingService.Api.Services.Implementation;
using SettingService.Api.Services.Interface;



namespace SettingService.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithRegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            services.AddScoped<IUserRoleService, Services.Implementation.UserRoleService>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<ILanguageService, LanguageService>();

            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();

            services.AddScoped<IMenuTypeRepository, MenuTypeRepository>();
            services.AddScoped<IMenuTypeService, MenuTypeService>();

            services.AddScoped<IMenuLanguageRepository, MenuLanguageRepository>();
            services.AddScoped<IMenuLanguageService, MenuLanguageService>();

            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();


            services.AddScoped<IMasterDropdownRepository, MasterDropdownRepository>();
            services.AddScoped<IMasterDropdownService, MasterDropdownService>();

            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();

            services.AddScoped<PgHelper>();

            return services;
        }
    }
}
