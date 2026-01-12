using Dapper;

using OrganizationService.Api.Infrastructure;
using OrganizationService.Api.ViewModels.Request.User;

using System.Data;

namespace OrganizationService.Api.Helpers
{
    public class TokenHelper
    {
        private readonly ILogger<TokenHelper> _logger;
        private readonly IDbConnectionFactory _dbFactory;

        public TokenHelper(IDbConnectionFactory dbFactory, ILogger<TokenHelper> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task<Guid> AddUserToken(UserTokenRequestViewModel tokenRequest)
        {
            _logger.LogInformation("Executing UserTokenCreate :{SPName}", StoreProcedure.UserTokenCreate);

            try
            {
                using var conn = _dbFactory.CreateConnection();

                var parameters = new DynamicParameters();

                parameters.Add("user_id", tokenRequest.User_Id);
                parameters.Add("email", tokenRequest.Email);
                parameters.Add("token", tokenRequest.Token);
                parameters.Add("token_data", tokenRequest.Token_data);
                parameters.Add("token_type", tokenRequest.Token_type);
                parameters.Add("token_expiry", tokenRequest.Token_expiry);
                parameters.Add("status", "1");
                parameters.Add("created_by", tokenRequest.User_Id);
                parameters.Add("ip_address", tokenRequest.Ip_address);
                parameters.Add("out_user_token_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

                await conn.ExecuteAsync(StoreProcedure.UserTokenCreate, parameters, commandType: CommandType.StoredProcedure);

                return parameters.Get<Guid>("out_user_token_id");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing UserTokenCreate :{SPName}", StoreProcedure.UserTokenCreate);
                throw;
            }
        }
    }
}
