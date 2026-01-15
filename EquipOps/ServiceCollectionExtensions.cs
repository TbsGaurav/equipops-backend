using CommonHelper.Helper;
using CommonHelper.Helpers;
using EquipOps.Model.FluentValidationModel;
using EquipOps.Serives.Implementation;
using EquipOps.Serives.Interface;
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
            return services;
        }
    }
}
