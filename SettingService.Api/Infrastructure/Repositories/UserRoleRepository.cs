using Dapper;

using Npgsql;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Request;
using SettingService.Api.ViewModels.Response.UserAccessRole;
using SettingService.Api.ViewModels.Response.UserRole;

using System.Data;
using System.Security.Claims;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ILogger<UserRoleRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly PgHelper _pghelper;

        public UserRoleRepository(IDbConnectionFactory dbFactory, ILogger<UserRoleRepository> logger, IHttpContextAccessor contextAccessor, PgHelper pghelper)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
            _pghelper = pghelper;
        }

        #region User Role
        public async Task<UserRoleResponseViewModel> UserRoleCreateAsync(UserRoleRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            var param = new Dictionary<string, DbParam>
            {
                // output params
                { "_return_id", new DbParam { Value = null, DbType = DbType.Guid, Direction = ParameterDirection.InputOutput } },
                { "_return_createddate", new DbParam { Value = null, DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },
                { "_return_updateddate", new DbParam { Value = null, DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },

                { "_id", new DbParam { Value = request.id, DbType = DbType.Guid } },
                { "_name", new DbParam { Value = request.name, DbType = DbType.String } },
                { "_created_by", new DbParam { Value = updated_by, DbType = DbType.Guid } },
                { "_updated_by", new DbParam { Value = updated_by, DbType = DbType.Guid } },
                { "_is_delete", new DbParam { Value = false, DbType = DbType.Boolean } },
                { "_is_active", new DbParam { Value = true, DbType = DbType.Boolean } }
            };

            dynamic result = await _pghelper.CreateUpdateAsync("master.sp_user_role_create_update", param);

            // Map output parameters to entity
            var data = new UserRoleResponseViewModel
            {
                id = result._return_id,
                name = request.name,
                is_delete = false,
                is_active = true,
                created_by = updated_by,
                created_date = result._return_createddate,
                updated_by = updated_by,
                updated_date = result._return_updateddate,
            };
            // Return in ResponseViewModel wrapper
            return data;
        }


        public async Task<UserRoleListResponseViewModel> UserRoleListAsync(string? search, bool? Is_Active, int length, int page, string orderColumn, string orderDirection)
        {
            var Params = new Dictionary<string, DbParam>
            {
                { "p_search",         new DbParam { Value = search ?? "", DbType = DbType.String } },
                { "p_is_active",      new DbParam { Value = Is_Active, DbType = DbType.Boolean } },
                { "p_length",         new DbParam { Value = length, DbType = DbType.Int32 } },
                { "p_page",           new DbParam { Value = page, DbType = DbType.Int32 } },
                { "p_order_column",   new DbParam { Value = orderColumn, DbType = DbType.String } },
                { "p_order_direction",new DbParam { Value = orderDirection, DbType = DbType.String } },

                // Output Params
                { "o_total_records",  new DbParam { Value = 0, DbType = DbType.Int32, Direction = ParameterDirection.InputOutput } },
                { "ref",              new DbParam { Value = "mycursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
            };

            dynamic response = await _pghelper.ListAsync("master.sp_get_user_role_list", Params);

            // Fetch cursor data inside the same transaction
            UserRoleListResponseViewModel list = new UserRoleListResponseViewModel();
            list.TotalNumbers = response.o_total_records;

            foreach (var row in response.@ref)  // row is dynamic ExpandoObject
            {
                list.userRoleResponseViewModel.Add(new UserRoleResponseViewModel
                {
                    id = row.id,
                    name = row.name,
                    created_by = row.created_by,
                    created_date = row.created_date,
                    updated_by = row.updated_by,
                    updated_date = row.updated_date,
                    is_delete = row.is_delete,
                    is_active = row.is_active
                });
            }
            return list;
        }

        public async Task<UserRoleDeleteResponseViewModel> UserRoleDeleteAsync(UserRoleDeleteRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            var param = new Dictionary<string, DbParam>
            {
                // output params
                { "_return_id", new DbParam { Value = null, DbType = DbType.Guid, Direction = ParameterDirection.InputOutput } },
                { "_return_updateddate", new DbParam { Value = null, DbType = DbType.DateTime, Direction = ParameterDirection.InputOutput } },

                { "_id", new DbParam { Value = request.id, DbType = DbType.Guid } },
                { "_updated_by", new DbParam { Value = updated_by, DbType = DbType.Guid } },
                { "_is_delete", new DbParam { Value = true, DbType = DbType.Boolean } },
                { "_is_active", new DbParam { Value = false, DbType = DbType.Boolean } }
            };

            dynamic result = await _pghelper.CreateUpdateAsync("master.sp_user_role_Delete", param);
            // Map output parameters to entity
            var data = new UserRoleDeleteResponseViewModel
            {
                id = result._return_id
            };
            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task<UserRoleResponseViewModel> UserRoleByIdAsync(Guid? id)
        {
            var Params = new Dictionary<string, DbParam>
            {
                { "_id", new DbParam { Value = id ?? null, DbType = DbType.Guid } },
                { "ref", new DbParam { Value = "mycursor", DbType = DbType.String, Direction = ParameterDirection.InputOutput } }
            };

            dynamic response = await _pghelper.ListAsync("master.sp_get_user_role_byid", Params);

            UserRoleResponseViewModel userRoleResponseViewModel = new UserRoleResponseViewModel();
            dynamic obj = response.@ref[0];
            userRoleResponseViewModel.id = obj.id;
            userRoleResponseViewModel.name = obj.name;
            userRoleResponseViewModel.created_by = obj.created_by;
            userRoleResponseViewModel.created_date = obj.created_date;
            userRoleResponseViewModel.updated_by = obj.updated_by;
            userRoleResponseViewModel.updated_date = obj.updated_date;
            userRoleResponseViewModel.is_delete = obj.is_delete;
            userRoleResponseViewModel.is_active = obj.is_active;

            return userRoleResponseViewModel;
        }

        #endregion

        #region  User Access Role
        public async Task<UserAccessRoleCreateUpdateResponseViewModel> UserAccessRoleCreateAsync(UserAccessRoleRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id);
            parameters.Add("_organization_id", request.organization_id);
            parameters.Add("_user_id", request.user_id);
            parameters.Add("_menu_name", request.menu_name);
            parameters.Add("_create", request.create);
            parameters.Add("_update", request.update);
            parameters.Add("_delete", request.delete);
            parameters.Add("_view", request.view);
            parameters.Add("_created_by", updated_by);

            // Output
            parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);

            // Execute procedure
            await conn.ExecuteAsync(
                "CALL master.sp_user_role_access_create_update(@_return_id, @_id, @_organization_id, @_user_id, @_menu_name, @_create, @_update, @_delete, @_view,  @_created_by)",
                parameters
            );

            // Map output parameters to entity
            var data = new UserAccessRoleCreateUpdateResponseViewModel
            {
                id = parameters.Get<Guid?>("_return_id"),
                organization_id = request.organization_id,
                user_id = request.user_id,
                menu_name = request.menu_name,
                create = request.create,
                update = request.update,
                delete = request.delete,
                view = request.view,
            };

            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task<UserAccessRoleListResponseViewModel> UserAccessRoleListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_get_user_access_role_list(@p_search,@p_is_active, @p_length, @p_page, @p_order_column, @p_order_direction, @o_total_records, @ref)", conn, tran);

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
            UserAccessRoleListResponseViewModel list = new UserAccessRoleListResponseViewModel();
            list.TotalNumbers = (int)cmd.Parameters["o_total_records"].Value;

            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.userRoleResponseViewModel.Add(new UserAccessRoleResponseViewModel
                    {
                        id = reader.GetGuid(0),
                        organization_id = reader.GetGuid(1),
                        user_id = reader.GetGuid(2),
                        menu_name = reader.GetString(3),
                        create = reader.GetBoolean(4),
                        update = reader.GetBoolean(5),
                        delete = reader.GetBoolean(6),
                        view = reader.GetBoolean(7),
                        created_by = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                        created_date = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        updated_by = reader.IsDBNull(10) ? null : reader.GetGuid(10),
                        updated_date = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        is_delete = reader.GetBoolean(12),
                        is_active = reader.GetBoolean(13)
                    });
                }
            }

            await tran.CommitAsync();
            return list;
        }

        public async Task<UserAccessRoleDeleteResponseViewModel> UserAccessRoleDeleteAsync(UserAccessRoleDeleteRequestViewModel request)
        {
            var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updated_by = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id);
            parameters.Add("_updated_by", updated_by);

            // Execute procedure
            await conn.ExecuteAsync(
                "CALL master.sp_user_access_role_Delete(@_id, @_updated_by)",
                parameters
            );

            // Map output parameters to entity
            var data = new UserAccessRoleDeleteResponseViewModel
            {
                id = request.id
            };

            // Return in ResponseViewModel wrapper
            return data;
        }

        public async Task<UserAccessRoleResponseViewModel> UserAccessRoleByIdAsync(Guid? id)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_get_user_access_role_byid(@_id, @ref)", conn, tran);

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
            UserAccessRoleResponseViewModel userAccessRoleResponseViewModel = new UserAccessRoleResponseViewModel();
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    userAccessRoleResponseViewModel.id = reader.GetGuid(0);
                    userAccessRoleResponseViewModel.organization_id = reader.GetGuid(1);
                    userAccessRoleResponseViewModel.user_id = reader.GetGuid(2);
                    userAccessRoleResponseViewModel.menu_name = reader.GetString(3);
                    userAccessRoleResponseViewModel.create = reader.GetBoolean(4);
                    userAccessRoleResponseViewModel.update = reader.GetBoolean(5);
                    userAccessRoleResponseViewModel.delete = reader.GetBoolean(6);
                    userAccessRoleResponseViewModel.view = reader.GetBoolean(7);
                    userAccessRoleResponseViewModel.created_by = reader.IsDBNull(8) ? null : reader.GetGuid(8);
                    userAccessRoleResponseViewModel.created_date = reader.IsDBNull(9) ? null : reader.GetDateTime(9);
                    userAccessRoleResponseViewModel.updated_by = reader.IsDBNull(10) ? null : reader.GetGuid(10);
                    userAccessRoleResponseViewModel.updated_date = reader.IsDBNull(11) ? null : reader.GetDateTime(11);
                    userAccessRoleResponseViewModel.is_delete = reader.GetBoolean(12);
                    userAccessRoleResponseViewModel.is_active = reader.GetBoolean(13);
                }
            }

            await tran.CommitAsync();
            return userAccessRoleResponseViewModel;
        }
        #endregion
    }
}
