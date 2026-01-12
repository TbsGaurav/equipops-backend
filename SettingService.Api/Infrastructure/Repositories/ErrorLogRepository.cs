using Dapper;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ErrorLogRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task LogAsync(ErrorLogModel log)
        {
            using var conn = _connectionFactory.CreateConnection();

            string query = @"
        INSERT INTO master.error_log 
        (name, message, detail, user_id, request_data, organization_id, created_by) 
        VALUES 
        (@name, @message, @detail, @user_id, @request_data, @organization_id, @created_by)";

            await conn.ExecuteAsync(query, log);
        }
    }
}
