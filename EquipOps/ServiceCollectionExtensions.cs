using CommonHelper.Helper;
using CommonHelper.Helpers;
using EquipOps.Serives.Implementation;
using EquipOps.Serives.Interface;

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
            return services;
        }
    }
}
