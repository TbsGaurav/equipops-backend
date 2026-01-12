using Dapper;

using Npgsql;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Request.MenuLanguage;
using SettingService.Api.ViewModels.Response.MenuLanguage;

using System.Data;
using System.Security.Claims;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class MenuLanguageRepository(ILogger<MenuLanguageRepository> logger, IHttpContextAccessor contextAccessor, IDbConnectionFactory _dbFactory) : IMenuLanguageRepository
    {
        #region Create Update Menu Language
        public async Task<MenuLanguageCreateUpdateResponseViewModel> CreateUpdateMenuLanguageAsync(MenuLanguageCreateUpdateRequestViewModel request)
        {
            logger.LogInformation("Executing Menu Language CreateUpdate stored procedure for Key_name={Key_name}", request.Key_name);
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_language_id", request.Language_id);
            parameters.Add("p_key_name", request.Key_name);
            parameters.Add("p_translate_text", request.Translate_text);
            parameters.Add("p_created_by", createdBy);
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(StoreProcedure.MenuLanguageCreateUpdate, parameters, commandType: CommandType.StoredProcedure);

            var outUserId = parameters.Get<Guid>("o_id");

            return new MenuLanguageCreateUpdateResponseViewModel
            {
                Id = outUserId,
                Language_id = request.Language_id,
                Key_name = request.Key_name,
                Translate_text = request.Translate_text,
                Created_by = createdBy,
                Created_date = DateTime.UtcNow,
                Is_active = true,
                Is_delete = false,
                Updated_by = null,
                Updated_date = null
            };
        }
        #endregion

        #region Delete Menu Language
        public async Task DeleteMenuLanguageAsync(MenuLanguageDeleteRequestViewModel request)
        {
            logger.LogInformation("Deleting menuLanguage with Menu LanguageId={Id}", request.Id);
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            //execute delete stored procedure
            await conn.ExecuteAsync(
                StoreProcedure.DeleteMenuLanguage,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion

        #region Get Menu Language By Id
        public async Task<MenuLanguageData> GetMenuLanguageByIdAsync(Guid? Id)
        {
            if (Id == null)
                return null;

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetMenuLanguageById}(@p_id, @ref)",
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

            var result = conn.QueryFirstOrDefault<MenuLanguageData>(
                "FETCH ALL IN \"my_cursor\"",
                transaction: tran
            );

            tran.Commit();

            return result;
        }
        #endregion

        #region Get Menu Language List
        public async Task<MenuLanguageListResponseViewModel> GetMenuLanguageListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                            $"CALL {StoreProcedure.GetMenuLanguageList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction,@p_is_active, @o_total_numbers, @ref)",
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
            var menuLanguages = conn.Query<MenuLanguageData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            return new MenuLanguageListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                MenuLanguageData = menuLanguages
            };
        }
        #endregion

        #region Get Menu Language By Language
        public async Task<Dictionary<string, string>> GetMenuLanguageByLanguageAsync(Guid? languageId)
        {
            if (languageId == Guid.Empty)
                return new Dictionary<string, string>();

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetMenuLanguageByLanguage}(@p_language_id, @ref)",
                (NpgsqlConnection)conn
            );

            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_language_id", languageId);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "menu_language_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            await cmd.ExecuteNonQueryAsync();

            var data = conn.Query<MenuLanguageData>(
                "FETCH ALL IN \"menu_language_cursor\"",
                transaction: tran
            ).ToList();

            tran.Commit();

            return data.ToDictionary(
                x => x.Key_name,
                x => x.Translate_text
            );
        }
        #endregion

        #region Update Menu Language By Language (Bulk)
        public async Task MenuLanguageByLanguageUpdateAsync(MenuLanguageByLanguageUpdateRequestViewModel request)
        {
            logger.LogInformation("Updating Menu Language with Menu LanguageId={Id}", request.LanguageId);

            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var parameters = new DynamicParameters();

            parameters.Add("p_language_id", request.LanguageId);
            parameters.Add("p_updated_by", updatedBy);
            parameters.Add("p_bulk_data", request.Data.ToArray());

            //execute menu language bulk update stored procedure
            await conn.ExecuteAsync(
                StoreProcedure.MenuLanguageByLanguageUpdate,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion
    }
}
