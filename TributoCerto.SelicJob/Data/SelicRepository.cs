using System.Text;
using Dapper;
using TributoCerto.Selic.Data.Interfaces;
using TributoCerto.Selic.Dtos;

namespace TributoCerto.Selic.Data;

public class SelicRepository : ISelicRepository
{
    private readonly Database _database;

    public SelicRepository(IConfiguration configuration)
    {
        _database = new Database(configuration);
    }

    public async Task<IList<SelicAccumulatedDto>> GetAllSelicTaxes()
    {
        var res = await _database.ExecuteQuery<SelicAccumulatedDto>(@"SELECT * FROM selic_tax");
        return res.ToList();
    }

        public async Task InsertRows(SelicAccumulatedDto data)
    {
        var @params = new DynamicParameters();
        @params.Add("data", data.ConvertIntoSqlInsertValues());
        await _database.ExecuteQuery<SelicAccumulatedDto>(InsertRowQuery, @params);
    }
    
    public async Task InsertRows(IEnumerable<SelicAccumulatedDto> data)
    {
        if (!data.Any()) return;
        var query = new StringBuilder(InsertRowQuery);
        query.Append(string.Join(',', data.Select(d => d.ConvertIntoSqlInsertValues())));
        await _database.ExecuteQuery<SelicAccumulatedDto>(query.ToString());
    }

    public async Task UpdateRow(SelicAccumulatedDto data)
    {
        await _database.ExecuteQuery<SelicAccumulatedDto>(UpdateRowQuery, data.ToDynamicParamenters());
    }

    private const string InsertRowQuery = @"INSERT INTO selic_tax (date, value, accumulated_value, uu_id, data_criacao, ultima_atualizacao, payment_date) VALUES ";

    private const string UpdateRowQuery = @"
        UPDATE selic_tax
        SET value=@value, accumulated_value=@accValue, payment_date=@paymentDate::timestamp, ultima_atualizacao=@updated_at::timestamp
        WHERE id=@id
    ";
}