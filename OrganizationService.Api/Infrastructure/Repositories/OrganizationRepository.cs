using Common.Services.Helpers;

using Dapper;

using Npgsql;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Request.Organzation;
using OrganizationService.Api.ViewModels.Request.User;
using OrganizationService.Api.ViewModels.Response.Organization;
using OrganizationService.Api.ViewModels.Response.User;

using System.Data;
using System.Security.Claims;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly ILogger<OrganizationRepository> _logger;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public OrganizationRepository(IDbConnectionFactory dbFactory, ILogger<OrganizationRepository> logger, IHttpContextAccessor contextAccessor)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        #region Get Organizations List
        public async Task<OrganizationListResponseViewModel> GetOrganizationsAsync(
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetOrganizationList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @o_total_numbers, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)IsActive ?? DBNull.Value);

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

            // Execute procedure
            cmd.ExecuteNonQuery();

            var organizations = conn.Query<OrganizationData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new OrganizationListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                OrganizationData = organizations
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Get Organization By Id
        public async Task<OrganizationByIdResponseViewModel> GetOrganizationByIdAsync(Guid Id)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetOrganizationById}(@p_id, @ref, @ref_location, @ref_department)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_id", Id);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            var cursor_ref_locationParam = new NpgsqlParameter("ref_location", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor_location"
            };
            cmd.Parameters.Add(cursor_ref_locationParam);

            var cursor_ref_departmentParam = new NpgsqlParameter("ref_department", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor_department"
            };

            cmd.Parameters.Add(cursor_ref_departmentParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var organizations = conn.Query<Organization>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            var organization = organizations.FirstOrDefault();

            if (organization != null)
            {
                // 🔹 Fetch location (cursor 2)
                var locations = (await conn.QueryAsync<OrganizationLocationResponse>(
                    "FETCH ALL IN \"my_cursor_location\"",
                    transaction: tran
                )).ToList();

                organization.Locations = locations;

                // 🔹 Fetch department (cursor 3)
                var departments = (await conn.QueryAsync<OrganizationDepartmentResponse>(
                    "FETCH ALL IN \"my_cursor_department\"",
                    transaction: tran
                )).ToList();

                organization.Departments = departments;
            }

            tran.Commit();

            // Map output parameters to entity

            var data = new OrganizationByIdResponseViewModel
            {
                Organization = organization
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Create/Update Organization
        public async Task<OrganizationCreateUpdateResponseViewModel> CreateUpdateOrganizationAsync(OrganizationCreateUpdateRequestViewModel request)
        {
            try
            {
                _logger.LogInformation("Executing OrganizationCreateUpdate stored procedure for Email={Email}", request.Email);
                var userIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

                using var conn = _dbFactory.CreateConnection();

                var parameters = new DynamicParameters();

                var hashedPassword = PasswordHelper.HashPassword(request.Password);

                // Input
                parameters.Add("p_id", request.Id);
                parameters.Add("p_name", request.Name);
                parameters.Add("p_description", request.Description);
                parameters.Add("p_website_url", request.Website_url);
                parameters.Add("p_email", request.Email);
                parameters.Add("p_phone_no", request.Phone_no);
                parameters.Add("p_created_by", createdBy);
                parameters.Add("p_first_name", request.First_name);
                parameters.Add("p_last_name", request.Last_name);
                parameters.Add("p_hash_password", hashedPassword);
                parameters.Add("p_industry_type_id", request.Industry_type_id);
                parameters.Add("p_number_of_employees", request.Number_of_employees);
                parameters.Add("p_locations",
                    request.Locations == null || !request.Locations.Any()
                        ? null
                        : request.Locations.ToArray(),
                    dbType: DbType.Object
                );
                parameters.Add("p_departments",
                    request.Departments == null || !request.Departments.Any()
                        ? null
                        : request.Departments.ToArray(),
                    dbType: DbType.Object
                );

                // Output
                parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);
                parameters.Add("o_user_id", dbType: DbType.Guid, direction: ParameterDirection.Output);
                parameters.Add("o_out_email", dbType: DbType.String, direction: ParameterDirection.Output);

                // Execute procedure
                await conn.ExecuteAsync(
                    StoreProcedure.OrganizationCreateUpdate,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Output values
                var outOrgId = parameters.Get<Guid?>("o_id") ?? Guid.Empty;
                var outUserId = parameters.Get<Guid?>("o_user_id") ?? Guid.Empty;
                var outEmail = parameters.Get<string>("o_out_email") ?? string.Empty;

                // Map output parameters to entity
                var data = new OrganizationCreateUpdateResponseViewModel
                {
                    Id = outOrgId,
                    UserId = outUserId,
                    Out_Email = outEmail
                };

                // Return in ResponseViewModel wrapper
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Verify Organization Email
        public async Task<EmailVerifyResponseViewModel> VerifyRegistrationEmailAsync(EmailVerifyRequestViewModel request, string Email, Guid UserId)
        {
            _logger.LogInformation("Executing VerifyRegistrationEmailAsync Token={Token}", request.Token);
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("@p_token", request.Token);
            parameters.Add("@p_user_id", UserId);
            parameters.Add("@p_email", Email);

            // Output
            parameters.Add("@p_result_message", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.UserTokenVerify,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var resultMessage = parameters.Get<string>("@p_result_message");

            // Map output parameters to entity
            var data = new EmailVerifyResponseViewModel
            {
                Email = Email,
                Status = resultMessage
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Delete Organization
        public async Task DeleteOrganizationAsync(OrganizationDeleteRequestViewModel request)
        {
            _logger.LogInformation("Executing DeleteOrganizationAsync stored procedure for Id={Id}", request.Id);
            var organizationIdClaim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var updatedBy = string.IsNullOrWhiteSpace(organizationIdClaim) ? (Guid?)null : Guid.Parse(organizationIdClaim);

            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("p_id", request.Id);
            parameters.Add("p_updated_by", updatedBy);

            // Execute procedure
            await conn.ExecuteAsync(
                StoreProcedure.OrganizationDelete,
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        #endregion

        #region Get Organization Profile
        public async Task<ProfileResponseViewModel?> GetOrganizationProfileAsync(Guid orgId)
        {
            using var conn = _dbFactory.CreateConnection();

            _logger.LogInformation("Fetching organization profile for Id={Id}", orgId);


            const string query = @"SELECT 
                                    usr.id AS id,
                                    usr.first_name AS first_name,
                                    usr.last_name AS last_name,
                                    CONCAT(
                                        COALESCE(usr.first_name, ''),
                                        ' ',
                                        COALESCE(usr.last_name, '')
                                    ) AS full_name,
                                    usr.email AS email,
                                    usr.phone_number AS phone_no,
                                    usr.job_title AS job_title,
                                    usr.location AS location,
                                    usr.photo AS photo_url,
                                    usr_rl.name AS role
                                FROM master.user AS usr
                                INNER JOIN master.user_role AS usr_rl
                                    ON usr.role_id = usr_rl.id
                                WHERE usr.id = @Id
                                LIMIT 1;
                            ";


            var organization = await conn.QueryFirstOrDefaultAsync<ProfileResponseViewModel>(query, new { Id = orgId });

            if (organization == null)
                _logger.LogWarning("No organization found with Id={Id}", orgId);
            else
                _logger.LogInformation("Organization found for Id={Id}", orgId);

            return organization;
        }
        #endregion

        public async Task<string?> GetExistingProfilePhotoAsync(Guid userId)
        {
            using var conn = _dbFactory.CreateConnection();
            const string query = @"SELECT photo FROM master.user WHERE id = @id;";
            return await conn.QueryFirstOrDefaultAsync<string>(query, new { id = userId });
        }
        /* -------------------- UPDATE PROFILE -------------------- */
        public async Task UpdateProfileAsync(ProfileRequestViewModel model)
        {
            using var conn = _dbFactory.CreateConnection();

            const string query = @"UPDATE master.user
            SET 
                first_name = @first_name,
                last_name = @last_name,
                email = @email,
                phone_number = @phone_no,
                job_title = @job_title,
                location = @location,
                photo = @photo_url
            WHERE id = @id;";

            await conn.ExecuteAsync(query, new
            {
                model.first_name,
                model.last_name,
                model.email,
                model.phone_no,
                model.job_title,
                model.location,
                model.photo_url,
                model.id
            });
        }

        #region Get All Organizations List
        public async Task<OrganizationListResponseViewModel> GetAllOrganizationsAsync(
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetAllOrganizationList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @c_total_numbers, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            var parameters = new DynamicParameters();

            // Input
            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)IsActive ?? DBNull.Value);

            var totalParam = new NpgsqlParameter("c_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
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

            // Execute procedure
            cmd.ExecuteNonQuery();

            var organizations = conn.Query<OrganizationData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new OrganizationListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                OrganizationData = organizations
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Get Organizations Users List
        public async Task<UserListResponseViewModel> GetAllOrganizationUsers(Guid orgId,
            string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? IsActive = null)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetAllOrganizationUsersList}(@p_org_id, @p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @p_is_active, @o_total_numbers, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_org_id", (object?)orgId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);
            cmd.Parameters.AddWithValue("p_is_active", (object?)IsActive ?? DBNull.Value);

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

            // Execute procedure
            cmd.ExecuteNonQuery();

            var users = conn.Query<UserData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new UserListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                UserData = users
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Update Organization Status
        public async Task<(bool Success, string Message)> UpdateOrganizationStatusAsync(
   Guid organizationId,
   string action)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.UpdateOrganizationStatus}(@p_org_id, @p_action, @p_updated_by, @o_success, @o_message)",
                (NpgsqlConnection)conn
            );

            cmd.Transaction = (NpgsqlTransaction)tran;

            var userIdClaim = _contextAccessor.HttpContext?
                .User.FindFirst("user_id")?.Value;

            Guid updatedBy = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(userIdClaim))
                updatedBy = Guid.Parse(userIdClaim);

            cmd.Parameters.Add(new NpgsqlParameter("p_org_id", NpgsqlTypes.NpgsqlDbType.Uuid)
            {
                Value = organizationId
            });

            cmd.Parameters.Add(new NpgsqlParameter("p_action", NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = action?.Trim() ?? string.Empty
            });

            cmd.Parameters.Add(new NpgsqlParameter("p_updated_by", NpgsqlTypes.NpgsqlDbType.Uuid)
            {
                Value = updatedBy == Guid.Empty ? DBNull.Value : updatedBy
            });

            cmd.Parameters.Add(new NpgsqlParameter("o_success", NpgsqlTypes.NpgsqlDbType.Boolean)
            {
                Direction = ParameterDirection.InputOutput,
                Value = DBNull.Value
            });

            cmd.Parameters.Add(new NpgsqlParameter("o_message", NpgsqlTypes.NpgsqlDbType.Text)
            {
                Direction = ParameterDirection.InputOutput,
                Value = DBNull.Value
            });

            _logger.LogInformation(
                "Executing SP: sp_update_organization_status | OrgId={OrgId}, Action={Action}, UpdatedBy={UpdatedBy}",
                organizationId, action, updatedBy
            );

            await cmd.ExecuteNonQueryAsync();
            tran.Commit();

            bool success = cmd.Parameters["o_success"].Value != DBNull.Value
                           && (bool)cmd.Parameters["o_success"].Value;

            string message = cmd.Parameters["o_message"].Value?.ToString() ?? string.Empty;

            return (success, message);
        }
        #endregion

        #region Get Organizations List by status
        public async Task<OrganizationListResponseViewModel> GetOrganizationsByStatus(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", int? status = 0)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetOrganizationByStatus}(@p_status, @p_search, @p_length, @p_page, @p_order_column, @p_order_direction, @c_total_numbers, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;


            // Input
            cmd.Parameters.AddWithValue("p_status", (object?)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_search", (object?)Search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_length", Length);
            cmd.Parameters.AddWithValue("p_page", Page);
            cmd.Parameters.AddWithValue("p_order_column", OrderColumn);
            cmd.Parameters.AddWithValue("p_order_direction", OrderDirection);

            var totalParam = new NpgsqlParameter("c_total_numbers", NpgsqlTypes.NpgsqlDbType.Integer)
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

            // Execute procedure
            cmd.ExecuteNonQuery();

            var organizations = conn.Query<OrganizationData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new OrganizationListResponseViewModel
            {
                TotalNumbers = (int)totalParam.Value,
                OrganizationData = organizations
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

    }
}