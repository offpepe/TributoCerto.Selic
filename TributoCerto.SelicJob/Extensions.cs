using System.Globalization;
using Dapper;
using TributoCerto.Selic.Dtos;

namespace TributoCerto.Selic;

public static class Extensions
{
    public static string ConvertIntoSqlInsertValues(this SelicAccumulatedDto dto)
    {
        var actualDate = DateTime.Now.ToString("yyyy-MM-dd");
        return $"('{dto.Date:yyyy-MM-dd}'::timestamp, {dto.Value}, {dto.Accumulated_Value}, '{Guid.NewGuid()}'::uuid, '{actualDate}'::timestamp, '{actualDate}'::timestamp, '{dto.Payment_Date:yyyy-MM-dd}'::timestamp)";
    }
    
    public static DynamicParameters ToDynamicParamenters(this SelicAccumulatedDto dto)
    {
        var @params = new DynamicParameters();
        @params.Add("value", dto.Value);
        @params.Add("accValue", dto.Accumulated_Value);
        @params.Add("paymentDate", dto.Payment_Date);
        @params.Add("updated_at", DateTime.Now);
        @params.Add("id", dto.Id);
        return @params;
    }
    
    public static SelicAccumulatedDto MapToDto(this SelicDto dto)
    {
        return new SelicAccumulatedDto()
        {
            Date = DateTime.ParseExact(dto.Data, "dd/MM/yyyy", CultureInfo.CurrentCulture),
            Value = dto.Valor
        };
        
    } 
}