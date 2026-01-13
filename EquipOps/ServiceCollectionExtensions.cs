using CommonHelper.Helper;
using CommonHelper.Helpers;
using EquipOps.Services.Implementation;
using EquipOps.Services.Interface;

namespace EquipOps
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WithRegisterServices(this IServiceCollection services)
        {
			services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
			services.AddScoped<IPgHelper, PgHelper>(); 

			// Equipment
			services.AddScoped<IEquipmentService, EquipmentService>();

            // Vendor
            services.AddScoped<IVendorService, VendorService>();

            // Equipment
            services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();

            return services;
        }
    }
}
