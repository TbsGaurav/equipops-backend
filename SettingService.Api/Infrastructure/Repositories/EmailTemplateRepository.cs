using Dapper;

using Npgsql;

using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Request.EmailTemplate;
using SettingService.Api.ViewModels.Response.EmailTemplate;

using System.Data;
using System.Security.Claims;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class EmailTemplateRepository(ILogger<EmailTemplateRepository> logger, IHttpContextAccessor contextAccessor, IDbConnectionFactory _dbFactory) : IEmailTemplateRepository
    {
        public async Task<EmailTemplateCreateUpdateResponseViewModel> CreateUpdateEmailTemplateAsync(EmailTemplateCreateUpdateRequestViewModel request)
        {
            var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Input
            parameters.Add("_id", request.id, DbType.Guid);
            parameters.Add("_type", request.type);
            parameters.Add("_subject", request.subject);
            parameters.Add("_text", request.text);
            parameters.Add("_created_by", createdBy, DbType.Guid);

            // Output
            parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);

            // Execute procedure
            await conn.ExecuteAsync(
                "CALL master.sp_email_template_create_update(@_return_id, @_id, @_type, @_subject, @_text, @_created_by)",

                parameters
            );

            // Map output parameters to entity
            var data = new EmailTemplateCreateUpdateResponseViewModel
            {
                id = parameters.Get<Guid?>("_return_id"),
                type = request.type,
                subject = request.subject,
                text = request.text
            };

            // Return in ResponseViewModel wrapper
            return data;

        }

        public async Task<EmailTemplateDeleteResponseViewModel> EmailTemplateDeleteAsync(EmailTemplateDeleteRequestViewModel request)
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
                "CALL master.sp_email_template_delete(@_id, @_updated_by)",
                parameters
            );

            // Map output parameters to entity
            var data = new EmailTemplateDeleteResponseViewModel
            {
                id = request.id
            };

            // Return in ResponseViewModel wrapper
            return data;

        }

        public async Task<EmailTemplateListResponseViewModel> EmailTemplateListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_email_template_get_list(@p_search, @p_is_active, @p_length, @p_page, @p_order_column, @p_order_direction, @o_total_records, @ref)", conn, tran);

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
            var list = new EmailTemplateListResponseViewModel();

            list.TotalNumbers = (int)cmd.Parameters["o_total_records"].Value;
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {

                    list.EmailTemplateData.Add(new EmailTemplateResponseViewModel
                    {
                        id = reader.GetGuid(0),
                        type = reader.GetString(1),
                        subject = reader.GetString(2),
                        text = reader.GetString(3),
                        created_by = reader.IsDBNull(4) ? null : reader.GetGuid(4),
                        created_date = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                        updated_by = reader.IsDBNull(6) ? null : reader.GetGuid(6),
                        updated_date = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        is_delete = reader.GetBoolean(8),
                        is_active = reader.GetBoolean(9)
                    });
                }
            }

            await tran.CommitAsync();
            return list;
        }

        public async Task<EmailTemplateResponseViewModel> EmailTemplateByIdAsync(Guid id)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_email_template_get_byid(@_id, @ref)", conn, tran);

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
            EmailTemplateResponseViewModel emailTemplateResponseViewModel = new EmailTemplateResponseViewModel();
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    emailTemplateResponseViewModel.id = reader.GetGuid(0);
                    emailTemplateResponseViewModel.type = reader.GetString(1);
                    emailTemplateResponseViewModel.subject = reader.GetString(2);
                    emailTemplateResponseViewModel.text = reader.GetString(3);
                    emailTemplateResponseViewModel.created_by = reader.IsDBNull(4) ? null : reader.GetGuid(4);
                    emailTemplateResponseViewModel.created_date = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
                    emailTemplateResponseViewModel.updated_by = reader.IsDBNull(6) ? null : reader.GetGuid(6);
                    emailTemplateResponseViewModel.updated_date = reader.IsDBNull(7) ? null : reader.GetDateTime(7);
                    emailTemplateResponseViewModel.is_delete = reader.GetBoolean(8);
                    emailTemplateResponseViewModel.is_active = reader.GetBoolean(9);
                }
            }

            await tran.CommitAsync();
            return emailTemplateResponseViewModel;
        }
    }
}
