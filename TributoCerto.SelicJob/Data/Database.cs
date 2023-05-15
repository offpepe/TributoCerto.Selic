using System.Data.Common;
using Dapper;
using Npgsql;

namespace TributoCerto.Selic.Data;

public class Database
{
    private readonly DbConnection _connection;

    public Database(IConfiguration configuration)
    {
        _connection = new NpgsqlConnection(configuration.GetConnectionString("PSQL"));
    }

    public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, DynamicParameters? @params = null)
    {
        await _connection.OpenAsync();
        var response = await _connection.QueryAsync<T>(query, @params);
        await _connection.CloseAsync();
        return response ?? Enumerable.Empty<T>();
    }
}