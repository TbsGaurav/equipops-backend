using Dapper;

using Npgsql;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Request.Menu_type;
using SettingService.Api.ViewModels.Response.Menu_type;

using System.Data;
using System.Security.Claims;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class MenuTypeRepository(ILogger<MenuTypeRepository> logger, IHttpContextAccessor contextAccessor, IDbConnectionFactory _dbFactory) : IMenuTypeRepository
    {
        #region Create Update Menu Type
        public async Task<MenuTypeCreateUpdateResponseViewModel> CreateUpdateMenuTypeAsync(MenuTypeCreateUpdateRequestViewModel request)
        {
            logger.LogInformation("Executing Menu Type CreateUpdate stored procedure for Name={Name}", request.Name);
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_name", request.Name);
            parameters.Add("p_created_by", createdBy);
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(StoreProcedure.MenuTypeCreateUpdate, parameters, commandType: CommandType.StoredProcedure);

            var outUserId = parameters.Get<Guid>("o_id");

            return new MenuTypeCreateUpdateResponseViewModel
            {
                Id = outUserId,
                Name = request.Name,
                Created_by = createdBy,
                Created_date = DateTime.UtcNow,
                Is_active = true,
                Is_delete = false,
                Updated_by = null,
                Updated_date = null
            };
        }
        #endregion

        #region Delete Menu Type
        public async Task DeleteMenuTypeAsync(MenuTypeDeleteRequestViewModel request)
        {
            logger.LogInformation("Deleting menu type with MenuTypeId={Id}", request.Id);
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            //execute delete stored procedure
            await conn.ExecuteAsync(
                StoreProcedure.DeleteMenuType,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion
        #region Get Menu Type By Id
        public async Task<MenuTypeData> GetMenuTypeByIdAsync(Guid? Id)
        {
            if (Id == null)
                return null;

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetMenuTypeById}(@p_id, @ref)",
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

            var result = conn.QueryFirstOrDefault<MenuTypeData>(
                "FETCH ALL IN \"my_cursor\"",
                transaction: tran
            );

            tran.Commit();

            return result;
        }
        #endregion

        #region Get Menu Type List
        public async Task<MenuTypeListResponseViewModel> GetMenuTypeListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                            $"CALL {StoreProcedure.GetMenuTypeList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction,@p_is_active, @o_total_numbers, @ref)",
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
            var menuTypes = conn.Query<MenuTypeData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            return new MenuTypeListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                MenuTypeData = menuTypes
            };
        }
        #endregion


        #region Menu Permission
        public async Task<MenuPermissionCreateUpdateResponseViewModel> CreateUpdateMenuPermissionAsync(MenuPermissionCreateUpdateRequestViewModel request)
        {
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id, DbType.Guid);
            parameters.Add("_menu_type_id", request.menu_type_id, DbType.Guid);
            parameters.Add("_slug", request.slug);
            parameters.Add("_name", request.name);
            parameters.Add("_created_by", createdBy, DbType.Guid);

            // Output
            parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);

            // Execute procedure
            await conn.ExecuteAsync(
                "CALL master.sp_menu_permission_create_update(@_return_id, @_id, @_menu_type_id, @_slug,@_name, @_created_by)",

                parameters
            );

            // Map output parameters to entity
            var data = new MenuPermissionCreateUpdateResponseViewModel
            {
                id = parameters.Get<Guid?>("_return_id"),
                menu_type_id = request.menu_type_id,
                slug = request.slug,
                name = request.name
            };

            // Return in ResponseViewModel wrapper
            return data;

        }

        public async Task<MenuPermissionDeleteResponseViewModel> MenuPermissionDeleteAsync(MenuPermissionDeleteRequestViewModel request)
        {
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id);
            parameters.Add("_updated_by", updated_by);

            // Execute procedure
            await conn.ExecuteAsync(
                "CALL master.sp_menu_permission_delete(@_id, @_updated_by)",
                parameters
            );

            // Map output parameters to entity
            var data = new MenuPermissionDeleteResponseViewModel
            {
                id = request.id
            };

            // Return in ResponseViewModel wrapper
            return data;

        }

        public async Task<MenuPermissionListResponseViewModel> MenuPermissionListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_menu_permission_get_list(@p_search, @p_is_active, @p_length, @p_page, @p_order_column, @p_order_direction, @o_total_records, @ref)", conn, tran);

            cmd.Parameters.AddWithValue("p_search", NpgsqlTypes.NpgsqlDbType.Text, search ?? "");
            cmd.Parameters.AddWithValue("p_is_active", NpgsqlTypes.NpgsqlDbType.Boolean, IsActive ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", NpgsqlTypes.NpgsqlDbType.Integer, length);
            cmd.Parameters.AddWithValue("p_page", NpgsqlTypes.NpgsqlDbType.Integer, page);
            cmd.Parameters.AddWithValue("p_order_column", NpgsqlTypes.NpgsqlDbType.Text, orderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", NpgsqlTypes.NpgsqlDbType.Text, orderDirection);

            // OUT parameters
            var totalParam = new NpgsqlParameter("o_total_records", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = ParameterDirection.InputOutput,
                Value = 0
            };
            cmd.Parameters.Add(totalParam);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = cursorName
            };
            cmd.Parameters.Add(cursorParam);

            // Execute the procedure
            await cmd.ExecuteNonQueryAsync();

            // Fetch cursor data inside the same transaction
            var list = new MenuPermissionListResponseViewModel();

            list.TotalNumbers = (int)cmd.Parameters["o_total_records"].Value;
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {

                    list.MenuPermissionData.Add(new MenuPermissionResponseViewModel
                    {
                        id = reader.GetGuid(0),
                        menu_type_id = reader.GetGuid(1),
                        slug = reader.GetString(2),
                        name = reader.IsDBNull(3) ? null : reader.GetString(3),
                        created_by = reader.IsDBNull(4) ? null : reader.GetGuid(4),
                        created_date = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                        updated_by = reader.IsDBNull(6) ? null : reader.GetGuid(6),
                        updated_date = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        is_delete = reader.GetBoolean(8),
                        is_active = reader.GetBoolean(9),
                        menu_type_name = reader.IsDBNull(10) ? null : reader.GetString(10)
                    });
                }
            }

            await tran.CommitAsync();
            return list;
        }

        public async Task<MenuPermissionResponseViewModel> MenuPermissionByIdAsync(Guid? id)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_menu_permission_get_byid(@_id, @ref)", conn, tran);

            cmd.Parameters.AddWithValue("_id", NpgsqlTypes.NpgsqlDbType.Uuid, id);

            // OUT parameters
            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = cursorName
            };
            cmd.Parameters.Add(cursorParam);

            // Execute the procedure
            await cmd.ExecuteNonQueryAsync();

            // Fetch cursor data inside the same transaction
            MenuPermissionResponseViewModel menuPermissionResponseViewModel = new MenuPermissionResponseViewModel();
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    menuPermissionResponseViewModel.id = reader.GetGuid(0);
                    menuPermissionResponseViewModel.menu_type_id = reader.GetGuid(1);
                    menuPermissionResponseViewModel.slug = reader.GetString(2);
                    menuPermissionResponseViewModel.name = reader.IsDBNull(3) ? null : reader.GetString(3);
                    menuPermissionResponseViewModel.created_by = reader.IsDBNull(4) ? null : reader.GetGuid(4);
                    menuPermissionResponseViewModel.created_date = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
                    menuPermissionResponseViewModel.updated_by = reader.IsDBNull(6) ? null : reader.GetGuid(6);
                    menuPermissionResponseViewModel.updated_date = reader.IsDBNull(7) ? null : reader.GetDateTime(7);
                    menuPermissionResponseViewModel.is_delete = reader.GetBoolean(8);
                    menuPermissionResponseViewModel.is_active = reader.GetBoolean(9);
                    menuPermissionResponseViewModel.menu_type_name = reader.IsDBNull(10) ? null : reader.GetString(10);
                }
            }

            await tran.CommitAsync();
            return menuPermissionResponseViewModel;
        }
        #endregion
    }
}
