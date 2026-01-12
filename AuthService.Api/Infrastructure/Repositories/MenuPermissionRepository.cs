using AuthService.Api.Infrastructure.Interface;

using Npgsql;

using System.Data;

namespace AuthService.Api.Infrastructure.Repositories
{
    public class MenuPermissionRepository : IMenuPermissionRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public MenuPermissionRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<string[]> GetPermissionList(Guid? UserId)
        {
            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "mycursor";

            await using var cmd = new NpgsqlCommand("CALL master.sp_menu_permission_Login_list(@_user_id, @ref)", conn, tran);

            cmd.Parameters.AddWithValue("_user_id", NpgsqlTypes.NpgsqlDbType.Uuid, UserId);

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
            var menuPermissions = new List<string>();
            await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran))
            await using (var reader = await fetchCmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {
                    menuPermissions.Add(reader.GetString(1)); // slug
                }
            }
            string[] menuPermissionArray = menuPermissions.ToArray();
            await tran.CommitAsync();
            return menuPermissionArray;
        }
    }
}
