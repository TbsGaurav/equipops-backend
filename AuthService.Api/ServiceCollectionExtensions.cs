using AuthService.Api.Infrastructure;
using AuthService.Api.Infrastructure.Interface;
using AuthService.Api.Infrastructure.Repositories;
using AuthService.Api.Services.Interface;

using Common.Services.Services.Implementation;
using Common.Services.Services.Interface;




namespace AuthService.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithRegisterServices(this IServiceCollection services)
        {


            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
			services.AddHttpClient();

			services.AddScoped<IAuthService, Services.Implementation.AuthService>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
            services.AddScoped<IMenuPermissionRepository, MenuPermissionRepository>();

            return services;
        }
    }
}
