using TributoCerto.Selic.Dtos;

namespace TributoCerto.Selic.Data.Interfaces;

public interface ISelicRepository
{
    Task<IList<SelicAccumulatedDto>> GetAllSelicTaxes();
    Task InsertRows(SelicAccumulatedDto data);
    Task InsertRows(IEnumerable<SelicAccumulatedDto> data);
    Task UpdateRow(SelicAccumulatedDto data);
}