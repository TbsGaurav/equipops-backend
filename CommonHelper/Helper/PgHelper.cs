using CommonHelper.Helper;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Dynamic;
using static CommonHelper.Helpers.PgHelper;

namespace CommonHelper.Helpers
{
	public class PgHelper: IPgHelper
	{
		private readonly IDbConnectionFactory _dbFactory;
        public PgHelper(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<dynamic> CreateUpdateAsync(string procedureName, Dictionary<string, DbParam> Params)
        {
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            // Add dynamic input or output
            foreach (var p in Params)
            {
                if (p.Key.Contains("return"))   // detect output params
                    parameters.Add(
                        p.Key,
                        value: null,
                        dbType: p.Value.DbType,   // detect type dynamically
                        direction: ParameterDirection.InputOutput
                    );
                else
                    parameters.Add(
                        p.Key,
                        value: p.Value.Value ?? (p.Value.Direction == ParameterDirection.InputOutput ? DBNull.Value : null),
                        dbType: p.Value.DbType,
                        direction: ParameterDirection.Input
                    );
            }

            // Build dynamic CALL query
            string query = BuildCallQuery(procedureName, Params);
            // Execute
            await conn.ExecuteAsync(query, parameters);

            var dynamicvalue = MapOutputAnonymous(parameters, Params);
            return dynamicvalue;
        }

        public async Task<dynamic> ListAsync(string procedureName, Dictionary<string, DbParam> Params)
        {
            dynamic dy = null;

            await using var conn = _dbFactory.CreateConnection() as NpgsqlConnection;
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var cursorName = "";

            // Build param list dynamically:  @p_search, @p_is_active, ...
            var paramNames = string.Join(", ", Params.Keys.Select(x => "@" + x));
            // Final command: CALL master.sp_get_user_role_list(@p_search, @p_is_active, ...)
            var sql = $"CALL {procedureName}({paramNames})";

            await using var cmd = new NpgsqlCommand(sql, conn, tran);

            foreach (var p in Params)
            {
                if ((p.Key.ToLower().Contains("ref") || p.Key.ToLower().Contains("cursor"))
                && (p.Value.Direction == ParameterDirection.Output || p.Value.Direction == ParameterDirection.InputOutput))
                {
                    cursorName = p.Value.Value.ToString();
                }
                var param = new NpgsqlParameter
                {
                    ParameterName = p.Key,
                    Direction = p.Value.Direction,
                    Value = p.Value.Value ?? DBNull.Value,
                    NpgsqlDbType = ConvertDbTypeToNpgsql(p.Value.DbType, p.Key, p.Value.Direction)
                };

                cmd.Parameters.Add(param);
            }
            // Execute the procedure
            await cmd.ExecuteNonQueryAsync();

            // Fetch cursor data inside the same transaction
            dynamic result = new ExpandoObject();
            IDictionary<string, object?> expando = new ExpandoObject();
            var listDict = (IDictionary<string, object?>)result;

            var outputParams = Params
                .Where(p => p.Value.Direction == ParameterDirection.InputOutput
                    || p.Value.Direction == ParameterDirection.Output);

            foreach (var p in outputParams)
            {
                var value = cmd.Parameters.Contains(p.Key)
                    ? cmd.Parameters[p.Key].Value
                    : null;

                expando[p.Key] = value is DBNull ? null : value;
            }

            // ------------------------------
            // Step 2️⃣: Fetch REF CURSOR data dynamically
            var cursorParams = Params
                .Where(p => (p.Key.ToLower().Contains("ref") || p.Key.ToLower().Contains("cursor"))
                            && (p.Value.Direction == ParameterDirection.Output || p.Value.Direction == ParameterDirection.InputOutput));

            foreach (var cp in cursorParams)
            {
                var curName = cmd.Parameters[cp.Key].Value?.ToString() ?? "";
                var rows = new List<dynamic>();

                await using (var fetchCmd = new NpgsqlCommand($"FETCH ALL FROM \"{curName}\";", conn, tran))
                await using (var reader = await fetchCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dynamic row = new ExpandoObject();
                        var rowDict = (IDictionary<string, object?>)row;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            rowDict[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }

                        rows.Add(row);
                    }
                }

                // Store each cursor result dynamically by cursor parameter name
                expando[cp.Key] = rows;
            }

            await tran.CommitAsync();
            var result1 = ConvertDictionaryToDynamic(expando);
            return result1;
        }

        private static NpgsqlTypes.NpgsqlDbType ConvertDbTypeToNpgsql(DbType type, string paramName, ParameterDirection direction)
        {
            // Special case: if it's an output/input-output parameter named "ref", treat as Refcursor
            if ((paramName.ToLower().Contains("ref") || paramName.ToLower().Contains("cursor"))
                && (direction == ParameterDirection.Output || direction == ParameterDirection.InputOutput))
            {
                return NpgsqlTypes.NpgsqlDbType.Refcursor;
            }

            return type switch
            {
                DbType.Guid => NpgsqlTypes.NpgsqlDbType.Uuid,
                DbType.String => NpgsqlTypes.NpgsqlDbType.Text,
                DbType.Boolean => NpgsqlTypes.NpgsqlDbType.Boolean,
                DbType.Int32 => NpgsqlTypes.NpgsqlDbType.Integer,
                DbType.DateTime => NpgsqlTypes.NpgsqlDbType.TimestampTz,
                _ => NpgsqlTypes.NpgsqlDbType.Text // fallback
            };
        }

        private string BuildCallQuery(string procedureName, Dictionary<string, DbParam> inputParams)
        {
            // get parameters in sequence like @_id, @_name, @_return_id ...
            var paramList = inputParams.Keys.Select(k => "@" + k);

            // join them by comma
            var joinedParams = string.Join(", ", paramList);

            // return dynamic CALL procedure
            return $"CALL {procedureName}({joinedParams})";
        }

        public static dynamic MapOutputAnonymous(DynamicParameters parameters, Dictionary<string, DbParam> paramDict)
        {
            var outputParams = paramDict
                .Where(p => p.Value.Direction == ParameterDirection.InputOutput
                    || p.Value.Direction == ParameterDirection.Output);

            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (var p in outputParams)
            {
                var value = parameters.ParameterNames.Contains(p.Key)
                    ? parameters.Get<object>(p.Key)
                    : null;

                expando[p.Key] = value == DBNull.Value ? null : value;
            }
            return expando;
        }

        public static dynamic ConvertDictionaryToDynamic(IDictionary<string, object?> dict)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = (IDictionary<string, object?>)expando;

            foreach (var kv in dict)
            {
                expandoDict[kv.Key] = kv.Value;
            }
            return expando;
        }
    }

    public class DbParam
    {
        public object? Value { get; set; }
        public DbType DbType { get; set; } = DbType.String;
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    }
}
