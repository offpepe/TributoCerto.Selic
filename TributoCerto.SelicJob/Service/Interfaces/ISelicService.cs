using TributoCerto.Selic.Dtos;

namespace TributoCerto.Selic.Service.Interfaces;

public interface ISelicService
{
    Task SeedDatabaseWithSelicTaxes(int yearsRange);
    Task<IList<SelicAccumulatedDto>> SearchSelicTax();
}