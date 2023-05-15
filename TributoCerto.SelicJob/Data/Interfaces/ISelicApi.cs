using TributoCerto.Selic.Dtos;

namespace TributoCerto.Selic.Data.Interfaces;

public interface ISelicApi
{
    Task<IList<SelicDto>> GetSelicTax(DateTime startDate, DateTime endDate);
}