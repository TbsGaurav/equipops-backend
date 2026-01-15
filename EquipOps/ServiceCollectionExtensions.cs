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

            // Equipment
            services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();

            // Equipment Failure
            services.AddScoped<IEquipmentFailureService, EquipmentFailureService>();

            // Organization
            services.AddScoped<IOrganizationService1, OrganizationService1>();

            return services;
        }
    }
}
