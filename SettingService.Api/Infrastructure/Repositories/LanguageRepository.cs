using Dapper;

using Npgsql;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Request.Language;
using SettingService.Api.ViewModels.Response.Language;

using System.Data;
using System.Security.Claims;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class LanguageRepository(ILogger<LanguageRepository> logger, IHttpContextAccessor contextAccessor, IDbConnectionFactory _dbFactory) : ILanguageRepository
    {
        public async Task<LanguageCreateUpdateResponseViewModel> CreateUpdateLanguageAsync(LanguageCreateUpdateRequestViewModel request)
        {
            logger.LogInformation("Executing language CreateUpdate stored procedure for Name={Name}", request.Name);


            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_name", request.Name);
            parameters.Add("p_code", request.Code);
            parameters.Add("p_direction", request.Direction);
            parameters.Add("p_created_by", createdBy);
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(StoreProcedure.LanguageCreateUpdate, parameters, commandType: CommandType.StoredProcedure);

            var outUserId = parameters.Get<Guid>("o_id");

            return new LanguageCreateUpdateResponseViewModel
            {
                Id = outUserId,
                Name = request.Name,
                Code = request.Code,
                Direction = request.Direction
            };
        }

        public async Task DeleteLanguageAsync(LanguageDeleteRequestViewModel request)
        {
            logger.LogInformation("Deleting language with languageId={languageId}", request.Id);

            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            //execute delete stored procedure
            await conn.ExecuteAsync(
                StoreProcedure.DeleteLanguage,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<LanguageData> GetLanguageByIdAsync(Guid? Id)
        {
            if (Id == null)
                return null;

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetLanguageById}(@p_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_id", Id.Value);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            await cmd.ExecuteNonQueryAsync();

            var result = conn.QueryFirstOrDefault<LanguageData>(
                "FETCH ALL IN \"my_cursor\"",
                transaction: tran
            );

            tran.Commit();

            return result;
        }

        public async Task<LanguageListResponseViewModel> GetLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                            $"CALL {StoreProcedure.GetLanguageList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction,@p_is_active, @o_total_numbers, @ref)",
                            (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)isActive ?? DBNull.Value);

            var totalParam = new NpgsqlParameter("o_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(totalParam);
            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);
            cmd.ExecuteNonQuery();
            var languages = conn.Query<LanguageData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            return new LanguageListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                LanguageData = languages
            };
        }
    }
}
