using Npgsql;

using System.Data;

namespace AuthService.Api.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_config.GetConnectionString("AuthConnection"));
        }
    }
}
