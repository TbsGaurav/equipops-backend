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
            services.AddScoped<IVendorService, VendorService>();

			// Dashboard Category
			services.AddValidatorsFromAssemblyContaining<DashboardCategoryValidator>();
			services.AddScoped<IDashboardCategoryService, DashboardCategoryService>();

            // Equipment Failure
            services.AddScoped<IEquipmentFailureService, EquipmentFailureService>();

            // Equipment Category
            services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();

            // Organization
            services.AddScoped<IOrganizationService1, OrganizationService1>();

            // Equipment Subparts
            services.AddScoped<IEquipmentSubpartService, EquipmentSubpartService>();

            // Audit Logs
            services.AddScoped<IAuditLogService, AuditLogService>();

            return services;
        }
    }
}
