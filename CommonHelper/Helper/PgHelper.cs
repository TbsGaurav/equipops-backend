using CommonHelper.Helper;
using Dapper;
using Npgsql;
using System.Data;
using System.Dynamic;

namespace CommonHelper.Helpers
{
    public class PgHelper(IDbConnectionFactory dbFactory) : IPgHelper
    {
        public async Task<dynamic> CreateUpdateAsync(string procedureName, Dictionary<string, DbParam> Params)
        {
            using var conn = dbFactory.CreateConnection();

            var parameters = new DynamicParameters();

            foreach (var p in Params)
            {
                if (p.Key.Contains("return"))
                    parameters.Add(
                        p.Key,
                        value: null,
                        dbType: p.Value.DbType,
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

            string query = BuildCallQuery(procedureName, Params);
            await conn.ExecuteAsync(query, parameters);

            var dynamicvalue = MapOutputAnonymous(parameters, Params);
            return dynamicvalue;
        }

        public async Task<dynamic> ListAsync(string procedureName,Dictionary<string, DbParam> Params)
        {
            await using var conn = dbFactory.CreateConnection() as NpgsqlConnection
                ?? throw new InvalidOperationException("Failed to create DB connection.");

            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            await using var cmd = BuildCommand(conn, tran, procedureName, Params);
            await cmd.ExecuteNonQueryAsync();

            var result = new ExpandoObject();
            var expando = (IDictionary<string, object?>)result;

            ReadOutputParameters(cmd, Params, expando);
            await ReadCursorResultsAsync(cmd, conn, tran, Params, expando);

            await tran.CommitAsync();
            return ConvertDictionaryToDynamic(expando);
        }

        private NpgsqlCommand BuildCommand(NpgsqlConnection conn,NpgsqlTransaction tran,string procedureName,Dictionary<string, DbParam> parameters)
        {
            var paramNames = string.Join(", ", parameters.Keys.Select(k => "@" + k));
            var sql = $"CALL {procedureName}({paramNames})";

            var cmd = new NpgsqlCommand(sql, conn, tran);

            foreach (var p in parameters)
            {
                cmd.Parameters.Add(new NpgsqlParameter
                {
                    ParameterName = p.Key,
                    Direction = p.Value.Direction,
                    Value = p.Value.Value ?? DBNull.Value,
                    NpgsqlDbType = ConvertDbTypeToNpgsql(
                        p.Value.DbType,
                        p.Key,
                        p.Value.Direction)
                });
            }

            return cmd;
        }
        private static void ReadOutputParameters( NpgsqlCommand cmd,Dictionary<string, DbParam> parameters,IDictionary<string, object?> expando)
        {
            foreach (var paramName in parameters
                .Where(p =>
                    p.Value.Direction == ParameterDirection.Output ||
                    p.Value.Direction == ParameterDirection.InputOutput)
                .Select(p => p.Key))
            {
                var npgsqlParam = cmd.Parameters.Contains(paramName)
                    ? cmd.Parameters[paramName]
                    : null;

                expando[paramName] =
                    npgsqlParam?.Value is DBNull ? null : npgsqlParam?.Value;
            }
        }

        private static async Task ReadCursorResultsAsync(NpgsqlCommand cmd,NpgsqlConnection conn,NpgsqlTransaction tran,Dictionary<string, DbParam> parameters,IDictionary<string, object?> expando)
        {
            var cursorKeys = parameters
                .Where(p =>
                    (p.Key.Contains("ref", StringComparison.OrdinalIgnoreCase) ||
                     p.Key.Contains("cursor", StringComparison.OrdinalIgnoreCase)) &&
                    p.Value.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                .Select(p => p.Key);

            foreach (var paramKey in cursorKeys)
            {
                var cursorName = cmd.Parameters[paramKey].Value?.ToString();
                if (string.IsNullOrWhiteSpace(cursorName))
                    continue;

                var rows = new List<dynamic>();

                await using var fetchCmd =
                    new NpgsqlCommand($"FETCH ALL FROM \"{cursorName}\";", conn, tran);

                await using var reader = await fetchCmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new ExpandoObject();
                    var rowDict = (IDictionary<string, object?>)row;

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        rowDict[reader.GetName(i)] =
                            await reader.IsDBNullAsync(i)
                                ? null
                                : reader.GetValue(i);
                    }

                    rows.Add(row);
                }

                expando[paramKey] = rows;
            }
        }

        private static NpgsqlTypes.NpgsqlDbType ConvertDbTypeToNpgsql(DbType type, string paramName, ParameterDirection direction)
        {
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
                _ => NpgsqlTypes.NpgsqlDbType.Text
            };
        }

        private static string BuildCallQuery(string procedureName, Dictionary<string, DbParam> inputParams)
        {
            var paramList = inputParams.Keys.Select(k => "@" + k);

            var joinedParams = string.Join(", ", paramList);

            return $"CALL {procedureName}({joinedParams})";
        }

        public static dynamic MapOutputAnonymous(DynamicParameters parameters,Dictionary<string, DbParam> paramDict)
        {
            var outputKeys = paramDict
                .Where(p =>
                    p.Value.Direction == ParameterDirection.Output ||
                    p.Value.Direction == ParameterDirection.InputOutput)
                .Select(p => p.Key);

            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (var key in outputKeys)
            {
                var value = parameters.ParameterNames.Contains(key)
                    ? parameters.Get<object>(key)
                    : null;

                expando[key] = value == DBNull.Value ? null : value;
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
