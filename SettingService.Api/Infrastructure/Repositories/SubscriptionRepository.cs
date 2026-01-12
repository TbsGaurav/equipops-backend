using Dapper;

using Npgsql;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Request.Subscription;
using SettingService.Api.ViewModels.Request.SubscriptionType;
using SettingService.Api.ViewModels.Response.Subscription;
using SettingService.Api.ViewModels.Response.SubscriptionType;

using System.Data;
using System.Security.Claims;

namespace SettingService.Api.Infrastructure.Repositories
{
	public class SubscriptionRepository(ILogger<SubscriptionRepository> logger, IHttpContextAccessor contextAccessor, IDbConnectionFactory _dbFactory) : ISubscriptionRepository
	{
		#region Subscription
		public async Task<SubscriptionCreateUpdateResponseViewModel> CreateUpdateSubscriptionAsync(SubscriptionCreateUpdateRequestViewModel request)
		{
			logger.LogInformation("Executing Subscription CreateUpdate stored procedure for Organization_id={Organization_id}", request.Organization_id);

			var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
			using var conn = _dbFactory.CreateConnection();
			var parameters = new DynamicParameters();
			parameters.Add("p_id", request.Id);
			parameters.Add("p_organization_id", request.Organization_id);
			parameters.Add("p_subscription_type_id", request.Subscription_type_id);
			parameters.Add("p_date", request.Date);
			parameters.Add("p_resume_maching_pending", request.Resume_maching_pending);
			parameters.Add("p_interview_create_pending", request.Interview_create_pending);
			parameters.Add("p_interview_schedule_pending", request.Interview_schedule_pending);
			parameters.Add("p_created_by", createdBy);
			parameters.Add("o_id", dbType: DbType.Guid, direction: ParameterDirection.Output);

			await conn.ExecuteAsync(StoreProcedure.SubscriptionCreateUpdate, parameters, commandType: CommandType.StoredProcedure);

			var outUserId = parameters.Get<Guid>("o_id");

			return new SubscriptionCreateUpdateResponseViewModel
			{
				Id = outUserId,
				Subscription_type_id = request.Subscription_type_id,
				Date = request.Date,
				Organization_id = request.Organization_id,
				Interview_schedule_pending = request.Interview_schedule_pending,
				Resume_maching_pending = request.Resume_maching_pending,
				Interview_create_pending = request.Interview_create_pending,
			};
		}

		public async Task<SubscriptionListResponseViewModel> GetSubscriptionListAsync(string? Search, int Length, int Page, string OrderColumn, string OrderDirection = "Asc", bool? isActive = null)
		{
			using var conn = _dbFactory.CreateConnection();
			conn.Open();

			using var tran = conn.BeginTransaction();

			await using var cmd = new NpgsqlCommand(
							$"CALL {StoreProcedure.GetSubscriptionList}(@p_search, @p_length, @p_page, @p_order_column, @p_order_direction,@p_is_active, @o_total_numbers, @ref)",
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
			var subscriptions = conn.Query<SubscriptionData>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

			tran.Commit();

			return new SubscriptionListResponseViewModel
			{
				TotalNumbers = (int)totalParam.Value,
				SubscriptionData = subscriptions
			};
		}

		public async Task DeleteSubscriptionAsync(SubscriptionDeleteRequestViewModel request)
		{
			logger.LogInformation("Deleting user with subscriptionId={subscriptionId}", request.Id);
			var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var updatedBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);

			using var conn = _dbFactory.CreateConnection();
			var parameters = new DynamicParameters();
			parameters.Add("p_id", request.Id);
			parameters.Add("p_updated_by", updatedBy);

			//execute delete stored procedure
			await conn.ExecuteAsync(
				StoreProcedure.DeleteSubscription,
				parameters,
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task<SubscriptionData> GetSubscriptionByIdAsync(Guid? Id)
		{
			if (Id == null)
				return null;

			using var conn = _dbFactory.CreateConnection();
			conn.Open();

			using var tran = conn.BeginTransaction();

			await using var cmd = new NpgsqlCommand(
				$"CALL {StoreProcedure.GetSubscriptionById}(@p_id, @ref)",
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

			var result = conn.QueryFirstOrDefault<SubscriptionData>(
				"FETCH ALL IN \"my_cursor\"",
				transaction: tran
			);

			tran.Commit();

			return result;
		}

		public async Task<OrganizationSubscriptionResponseViewModel> GetSubscriptionByOrganizationIdAsync(Guid? OrganizationId)
		{
			await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
			await conn.OpenAsync();
			await using var tran = await conn.BeginTransactionAsync();
			var cursorName = "mycursor";

			await using var cmd = new NpgsqlCommand(
				$"CALL {StoreProcedure.GetSubscriptionByOrganizationId}(@p_organization_id, @ref)", conn, tran);

			cmd.Parameters.AddWithValue("p_organization_id", NpgsqlTypes.NpgsqlDbType.Uuid, OrganizationId);

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
			var subscription = conn.QueryFirstOrDefault<OrganizationSubscriptionResponseViewModel>(
				$"FETCH ALL IN \"{cursorName}\";", conn, tran);

			tran.Commit();

			return subscription;
		}
		#endregion

		#region Subscription Type

		public async Task<SubscriptionTypeCreateUpdateResponseViewModel> CreateUpdateSubscriptionTypeAsync(SubscriptionTypeCreateUpdateRequestViewModel request)
		{
			var userIdClaim = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var createdBy = string.IsNullOrWhiteSpace(userIdClaim) ? (Guid?)null : Guid.Parse(userIdClaim);
			using var conn = _dbFactory.CreateConnection();

			var parameters = new DynamicParameters();

			// Input
			parameters.Add("_id", request.id, DbType.Guid);
			parameters.Add("_type", request.type);
			parameters.Add("_resume_matching", request.resume_matching);
			parameters.Add("_interview_create", request.interview_create);
			parameters.Add("_interview_schedule", request.interview_schedule);
			parameters.Add("_price", request.price);
			parameters.Add("_duration", request.duration);
			parameters.Add("_description", request.description);
			parameters.Add("_created_by", createdBy, DbType.Guid);

			// Output
			parameters.Add("_return_id", dbType: DbType.Guid, direction: ParameterDirection.InputOutput);

			// Execute procedure
			await conn.ExecuteAsync(
				"CALL master.sp_subscription_type_create_update(@_return_id, @_id, @_type, @_resume_matching, @_interview_create, @_interview_schedule, @_price, @_duration, @_description, @_created_by)",

				parameters
			);

			// Map output parameters to entity
			var data = new SubscriptionTypeCreateUpdateResponseViewModel
			{
				id = parameters.Get<Guid?>("_return_id"),
				type = request.type,
				resume_matching = request.resume_matching,
				interview_create = request.interview_create,
				interview_schedule = request.interview_schedule,
				price = request.price,
				duration = request.duration,
				description = request.description,
			};

			// Return in ResponseViewModel wrapper
			return data;

		}

		public async Task<SubscriptionTypeDeleteResponseViewModel> SubscriptionTypeDeleteAsync(SubscriptionTypeDeleteRequestViewModel request)
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
				"CALL master.sp_subscription_type_delete(@_id, @_updated_by)",
				parameters
			);

			// Map output parameters to entity
			var data = new SubscriptionTypeDeleteResponseViewModel
			{
				id = request.id
			};

			// Return in ResponseViewModel wrapper
			return data;

		}

		public async Task<SubscriptionTypeListResponseViewModel> SubscriptionTypeListAsync(string? search, bool? IsActive, int length, int page, string orderColumn, string orderDirection)
		{
			await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
			await conn.OpenAsync();
			await using var tran = await conn.BeginTransactionAsync();
			var cursorName = "mycursor";

			await using var cmd = new NpgsqlCommand("CALL master.sp_get_subscription_type_list(@p_search, @p_is_active, @p_length, @p_page, @p_order_column, @p_order_direction, @o_total_records, @ref)", conn, tran);

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
			var list = new SubscriptionTypeListResponseViewModel();

			list.TotalNumbers = (int)cmd.Parameters["o_total_records"].Value;
			await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
			await using (var reader = await fetchCmd.ExecuteReaderAsync())
			{

				while (await reader.ReadAsync())
				{

					list.SubscriptionTypeData.Add(new SubscriptionTypeResponseViewModel
					{
						id = reader.GetGuid(0),
						type = reader.GetString(1),
						resume_matching = reader.GetInt32(2),
						interview_create = reader.GetInt32(3),
						interview_schedule = reader.GetInt32(4),
						price = reader.GetDecimal(5),
						duration = reader.GetInt32(6),
						description = reader.IsDBNull(7) ? null : reader.GetString(7),
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

		public async Task<SubscriptionTypeResponseViewModel> SubscriptionTypeByIdAsync(Guid id)
		{
			await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
			await conn.OpenAsync();
			await using var tran = await conn.BeginTransactionAsync();
			var cursorName = "mycursor";

			await using var cmd = new NpgsqlCommand("CALL master.sp_get_subscription_type_ById(@_id, @ref)", conn, tran);

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
			SubscriptionTypeResponseViewModel subscriptionTypeResponseViewModel = new SubscriptionTypeResponseViewModel();
			await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
			await using (var reader = await fetchCmd.ExecuteReaderAsync())
			{
				while (await reader.ReadAsync())
				{
					subscriptionTypeResponseViewModel.id = reader.GetGuid(0);
					subscriptionTypeResponseViewModel.type = reader.GetString(1);
					subscriptionTypeResponseViewModel.resume_matching = reader.GetInt32(2);
					subscriptionTypeResponseViewModel.interview_create = reader.GetInt32(3);
					subscriptionTypeResponseViewModel.interview_schedule = reader.GetInt32(4);
					subscriptionTypeResponseViewModel.price = reader.GetDecimal(5);
					subscriptionTypeResponseViewModel.duration = reader.GetInt32(6);
					subscriptionTypeResponseViewModel.description = reader.IsDBNull(7) ? null : reader.GetString(7);
					subscriptionTypeResponseViewModel.created_by = reader.IsDBNull(8) ? null : reader.GetGuid(8);
					subscriptionTypeResponseViewModel.created_date = reader.IsDBNull(9) ? null : reader.GetDateTime(9);
					subscriptionTypeResponseViewModel.updated_by = reader.IsDBNull(10) ? null : reader.GetGuid(10);
					subscriptionTypeResponseViewModel.updated_date = reader.IsDBNull(11) ? null : reader.GetDateTime(11);
					subscriptionTypeResponseViewModel.is_delete = reader.GetBoolean(12);
					subscriptionTypeResponseViewModel.is_active = reader.GetBoolean(13);

				}
			}

			await tran.CommitAsync();
			return subscriptionTypeResponseViewModel;
		}
		#endregion
	}
}
