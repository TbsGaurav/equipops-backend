using Dapper;

using Npgsql;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Helpers.EncryptionHelpers.Handlers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Request.OrganzationSetting;
using OrganizationService.Api.ViewModels.Response.OrganizationSetting;

using System.Data;
using System.Security.Claims;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class OrganizationSettingRepository : IOrganizationSettingRepository
    {
        private readonly ILogger<OrganizationSettingRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EncryptionHelper _encryptionHelper;

        public OrganizationSettingRepository(IDbConnectionFactory dbFactory, ILogger<OrganizationSettingRepository> logger, IHttpContextAccessor httpContextAccessor, EncryptionHelper encryptionHelper)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _encryptionHelper = encryptionHelper;

        }

        #region Get Organization Settings List
        public async Task<List<OrganizationSettingListResponseViewModel>> GetOrganizationSettingsAsync()
        {
            var orgIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;
            Guid organizationId = Guid.Parse(orgIdClaim);

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetOrganizationSettingList}(@p_organization_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            var parameters = new DynamicParameters();

            // Input
            cmd.Parameters.AddWithValue("p_organization_id", NpgsqlTypes.NpgsqlDbType.Uuid, organizationId);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var organizationSettings = conn.Query<OrganizationSettingListResponseViewModel>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Return in ResponseViewModel wrapper
            return organizationSettings;
        }
        #endregion

        #region Get Organization Setting By Key
        public async Task<OrganizationSettingByKeyResponseViewModel> GetOrganizationSettingByKeyAsync(string Key)
        {
            var orgIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;
            Guid organizationId = Guid.Parse(orgIdClaim);

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetOrganizationSettingByKey}(@p_key, @p_organization_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            var parameters = new DynamicParameters();

            // Input
            cmd.Parameters.AddWithValue("p_key", Key);
            cmd.Parameters.AddWithValue("p_organization_id", NpgsqlTypes.NpgsqlDbType.Uuid, organizationId);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var organizationSettings = conn.Query<OrganizationSettingByKeyResponseViewModel>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            var organizationSetting = organizationSettings.FirstOrDefault();

            tran.Commit();

            // Return in ResponseViewModel wrapper
            return organizationSetting;
        }
        #endregion

        #region Create/Update Organization Setting
        public async Task<OrganizationSettingInternalResult> CreateUpdateOrganizationSettingAsync(OrganizationSettingCreateUpdateRequestViewModel request)
        {
            _logger.LogInformation("Executing OrganizationSettingCreateUpdate stored procedure for Key={Key}", request.Key);
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var orgIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("organization_id")?.Value;
            Guid organizationId = Guid.Parse(orgIdClaim);

            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();


            var query = @"SELECT 1 
                  FROM master.organization_setting 
                  WHERE organization_id = @OrgId 
                    AND key = @Key
                  LIMIT 1";

            var isExists = await conn.ExecuteScalarAsync<int?>(query, new { OrgId = organizationId, Key = request.Key });

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_organization_id", organizationId);
            parameters.Add("p_key", request.Key);
            parameters.Add("p_value", request.Value);
            parameters.Add("p_created_by", createdBy);

            // Output
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.OrganizationSettingCreateUpdate,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            // Output values
            var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;

            // Map output parameters to entity
            var data = new OrganizationSettingCreateUpdateResponseViewModel
            {
                Id = outOrgId,
                Key = request.Key,
                Value = request.Value,
            };

            var result = new OrganizationSettingInternalResult
            {
                Data = data,
                Exists = isExists.HasValue
            };

            // Return in ResponseViewModel wrapper
            return result;
        }
        #endregion

        public async Task<string?> GetRetellAiKey(Guid organizationId)
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
              SELECT value
              FROM master.organization_Setting
              WHERE organization_id = @OrgId 
                AND key = 'retell_ai_api_key'
              LIMIT 1;
            ";

            var encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = organizationId });

            if (string.IsNullOrEmpty(encrypted))
                return null;

            var decrypted = _encryptionHelper.DecryptFromReact(encrypted);

            return decrypted;
        }
    }
}
