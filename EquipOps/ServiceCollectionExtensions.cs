using CommonHelper.Helper;
using CommonHelper.Helpers;
using EquipOps.Model.FluentValidationModel;
using EquipOps.Services.Implementation;
using EquipOps.Services.Interface;
using FluentValidation;

namespace EquipOps
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithRegisterServices(this IServiceCollection services)
        {
			services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
			services.AddScoped<IPgHelper, PgHelper>();

			// Equipment
			services.AddValidatorsFromAssemblyContaining<EquipmentRequestValidator>();
			services.AddScoped<IEquipmentService, EquipmentService>();

            // Vendor
            services.AddValidatorsFromAssemblyContaining<VendorRequestValidator>();
            services.AddScoped<IVendorService, VendorService>();

			// Dashboard Category
			services.AddValidatorsFromAssemblyContaining<DashboardCategoryValidator>();
			services.AddScoped<IDashboardCategoryService, DashboardCategoryService>();

            // Equipment Failure
            services.AddValidatorsFromAssemblyContaining<EquipmentFailureRequestValidator>();
            services.AddScoped<IEquipmentFailureService, EquipmentFailureService>();

            // Equipment Category
            services.AddValidatorsFromAssemblyContaining<EquipmentCategoryRequestValidator>();
            services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();

            // Organization
            services.AddScoped<IOrganizationService1, OrganizationService1>();

            // Equipment Subparts
            services.AddValidatorsFromAssemblyContaining<EquipmentSubpartRequestValidator>();
            services.AddScoped<IEquipmentSubpartService, EquipmentSubpartService>();

            // Audit Logs
            services.AddScoped<IAuditLogService, AuditLogService>();

            // Permission
            services.AddScoped<IPermissionService, PermissionService>();

            // Role
            services.AddScoped<IRoleService, RoleService>();

            // DowntimeLog
            services.AddScoped<IDowntimeLogService, DowntimeLogService>();

            // DashboardData
            services.AddScoped<IDashboardDataService, DashboardDataService>();

            return services;
        }
    }
}
