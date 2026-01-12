
using Common.Services.Helpers;

using Dapper;

using Npgsql;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.User;

using System.Data;
using System.Security.Claims;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class UserRepository(ILogger<UserRepository> _logger, IDbConnectionFactory _dbFactory, IHttpContextAccessor contextAccessor) : IUserRepository
    {
        #region Repository
        public async Task<UserCreateUpdateResponseViewModel> CreateUpdateUserAsync(UserCreateUpdateRequestViewModel request)
        {
            _logger.LogInformation("Executing User CreateUpdate stored procedure for Email={Email}", request.Email);
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();
            var isNewUser = !request.Id.HasValue || request.Id == Guid.Empty;
            string generatedPassword = null;
            string hashedPassword = null;

            var roleName = string.IsNullOrWhiteSpace(request.Role_Name) ? "User" : request.Role_Name;

            var parameters = new DynamicParameters();
            if (isNewUser)
            {
                generatedPassword = PasswordHelper.GenerateRandomPassword();
                hashedPassword = PasswordHelper.HashPassword(generatedPassword);
            }

            parameters.Add("p_id", request.Id);
            parameters.Add("p_first_name", request.First_Name);
            parameters.Add("p_last_name", request.Last_Name);
            parameters.Add("p_email", request.Email);
            parameters.Add("p_phone_number", request.Phone_Number);
            parameters.Add("p_organization_id", request.Organization_Id);
            parameters.Add("p_role_name", roleName);
            parameters.Add("p_language_id", request.Language_Id);
            parameters.Add("p_hash_password", isNewUser ? hashedPassword : null, DbType.String);
            parameters.Add("p_created_by", createdBy);
            parameters.Add("p_bulk_data", request.UserAccessRole.ToArray());
            parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);
            parameters.Add("o_out_email", dbType: DbType.String, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(StoreProcedure.UserCreateUpdate, parameters, commandType: CommandType.StoredProcedure);

            var outUserId = parameters.Get<Guid>("o_id");
            var outEmail = parameters.Get<string>("o_out_email");

            return new UserCreateUpdateResponseViewModel
            {
                Id = outUserId,
                Email = outEmail,
                First_Name = request.First_Name,
                Last_Name = request.Last_Name,
                Phone_Number = request.Phone_Number,
                Organization_Id = request.Organization_Id,
                Role_Name = roleName,
                Language_Id = request.Language_Id,
                Password = isNewUser ? generatedPassword : null
            };
        }

        public async Task DeleteUserAsync(UserDeleteRequestViewModel request)
        {
            _logger.LogInformation("Deleting user with UserId={UserId}", request.Id);

            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var conn = _dbFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            //execute delete stored procedure
            await conn.ExecuteAsync(
                StoreProcedure.DeleteUser,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<UserData> GetUserByIdAsync(Guid? Id)
        {
            if (Id == null)
                return null;

            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetUserById}(@p_id, @ref, @ref_permission)",
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
            var cursor_ref_permissionParam = new NpgsqlParameter("ref_permission", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor_permission"
            };

            cmd.Parameters.Add(cursor_ref_permissionParam);

            await cmd.ExecuteNonQueryAsync();

            var result = conn.QueryFirstOrDefault<UserData>(
                "FETCH ALL IN \"my_cursor\"",
                transaction: tran
            );

            if (result != null)
            {
                // 🔹 Fetch permissions (cursor 2)
                var permissions = (await conn.QueryAsync<UserAccessRoleDetail>(
                    "FETCH ALL IN \"my_cursor_permission\"",
                    transaction: tran
                )).ToList();

                result.UserAccessRole = permissions;
            }

            tran.Commit();

            return result;
        }

        public async Task<UserListResponseViewModel> GetUserListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", string role = "", bool? isActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            var userIdClaim = contextAccessor.HttpContext?.User?.FindFirst("user_id")?.Value;
            Guid? userId = string.IsNullOrEmpty(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

            using var tran = conn.BeginTransaction();

            using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetUserList}(" +
                "@p_user_id,@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, " +
                "@p_role, @p_is_active, @o_total_numbers, @ref)",
                (NpgsqlConnection)conn);

            cmd.Transaction = (NpgsqlTransaction)tran;

            cmd.Parameters.AddWithValue("p_user_id", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);
            cmd.Parameters.AddWithValue("p_role", role);
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

            var users = conn.Query<UserData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            return new UserListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                UserData = users
            };
        }

        public async Task<UserData?> GetUserProfile(string id)
        {

            using var conn = _dbFactory.CreateConnection();
            conn.Open(); // Use async

            _logger.LogInformation("Fetching user profile for Id={Id}", id);

            // Validate GUID
            if (!Guid.TryParse(id, out Guid userId))
            {
                _logger.LogWarning("Invalid GUID format for Id={Id}", id);
                return null;
            }

            // Start a transaction (required for refcursor)
            using var tran = conn.BeginTransaction();

            // Name of the cursor
            const string cursorName = "user_cursor";
            const string my_cursor_permission = "my_cursor_permission";

            // Call the stored procedure
            await using var cmd = new Npgsql.NpgsqlCommand("CALL master.sp_user_get_by_id(@p_id, @ref, @ref_permission)", (NpgsqlConnection)conn, (Npgsql.NpgsqlTransaction)tran);

            // Add parameters
            cmd.Parameters.AddWithValue("p_id", userId);
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = cursorName
            });
            cmd.Parameters.Add(new Npgsql.NpgsqlParameter("ref_permission", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = my_cursor_permission
            });

            await cmd.ExecuteNonQueryAsync();

            // Fetch all rows from the cursor
            var users = await conn.QueryAsync<UserData>($"FETCH ALL FROM \"{cursorName}\"", transaction: tran);

            tran.Commit();

            var userData = users.FirstOrDefault();

            if (userData == null)
                _logger.LogWarning("No user found with Id={Id}", id);
            else
                _logger.LogInformation("User found for Id={Id}", id);

            return userData;
        }

        #endregion
    }
}
