using TributoCerto.Selic.Data;
using TributoCerto.Selic.Data.Interfaces;
using TributoCerto.Selic.Dtos;
using TributoCerto.Selic.Service.Interfaces;

namespace TributoCerto.Selic.Service;

public class SelicService : ISelicService
{
    private readonly ISelicApi _api;
    private readonly ISelicRepository _repository;
    private readonly ILogger<SelicService> _logger;

    public SelicService(ISelicApi api, ISelicRepository repository, ILogger<SelicService> logger)
    {
        _api = api;
        _repository = repository;
        _logger = logger;
    }

    public async Task<IList<SelicAccumulatedDto>> SearchSelicTax()
    {
        return await _repository.GetAllSelicTaxes();
    }
    public async Task SeedDatabaseWithSelicTaxes(int yearsRange)
    {
        _logger.LogInformation("Iniciando processo de atualização da selic");
        var end = DateTime.Now;
        var start = new DateTime(end.Year - yearsRange, 1, 1);
        _logger.LogInformation("Buscando dados do ano {Start} até {End}", start.Year, end.Year);
        var inMemoryTaxes = await _repository.GetAllSelicTaxes();
        _logger.LogInformation("{NumRegistros} registros encontrados no banco de dados", inMemoryTaxes.Count);
        var bacenTaxes = (await _api.GetSelicTax(start, end))
            .Select(t => t.MapToDto())
            .ToList();
        _logger.LogInformation("{NumRegistros} registros retornados pela API do Bacen", bacenTaxes.Count);
        var newTaxes = new List<SelicAccumulatedDto>();
        _logger.LogInformation("Iniciando calculo de novos registros");
        for (var year = start.Year; year <= end.Year; year++)
        {
            for (var month = 1; month <= 12; month++)
            {
                if (inMemoryTaxes.Any(t => t.Date.Year == year && t.Date.Month == month)) continue;
                var bacenTax = bacenTaxes.FirstOrDefault(t => t.Date.Year == year && t.Date.Month == month);
                if (year == end.Year && month >= end.Month && end.Day != DateTime.DaysInMonth(year, month))
                {
                    newTaxes.Add(new SelicAccumulatedDto()
                    {
                        Value = 1,
                        Date = bacenTax?.Date ?? new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                        Payment_Date = (bacenTax?.Date ?? new DateTime(year, month, DateTime.DaysInMonth(year, month)))
                            .AddMonths(1),
                        Accumulated_Value = 0
                    });
                    break;
                }
                _logger.LogInformation("Calculando valor acumulado da taxa selic de {Mes}/{Ano}", month,year);
                var accVal = month == 12
                    ? CalculateAccumulatedSelic(1, year + 1, bacenTaxes)
                    : CalculateAccumulatedSelic(month + 1, year, bacenTaxes);
                _logger.LogInformation("Valor calculado com sucesso -> resultado: {Res}", accVal);
                newTaxes.Add(new SelicAccumulatedDto()
                {
                    Value = bacenTax?.Value ?? decimal.Zero,
                    Date = bacenTax?.Date ?? DateTime.Now,
                    Payment_Date = bacenTax?.Date.AddMonths(1) ?? DateTime.Now.AddMonths(1),
                    Accumulated_Value = accVal.Value
                });
                _logger.LogInformation("Calculo de {Mes}/{Ano} concluído", month, year);
            }
        }
        
        _logger.LogInformation("Atualizando registros encontrados no banco");
        foreach (var tax in inMemoryTaxes)
        {
            var bacenTax =
                bacenTaxes.FirstOrDefault(t => t.Date.Year == tax.Date.Year && t.Date.Month == tax.Date.Month);
            if (bacenTax != null) tax.Value = bacenTax.Value;
            tax.Payment_Date = tax.Date.AddMonths(1);
            if (tax.Date.Month == end.Month && tax.Date.Year == end.Year)
            {
                tax.Accumulated_Value = 1;
                break;
            }
            _logger.LogInformation("Re-calculando o valor acumulado da taxa selic de {Data}", tax.Date.ToString("MM/yy"));
            var accumaltedTax = CalculateAccumulatedSelic(tax.Date.Month, tax.Date.Year, bacenTaxes);
            tax.Accumulated_Value = accumaltedTax.Value;
            _logger.LogInformation("Taxa calculada com sucesso -> resultado: {Res}", accumaltedTax.Value);
            _logger.LogInformation("Iniciando atualização");
            await _repository.UpdateRow(tax);
            _logger.LogInformation("Taxa de {Data} atualizada com sucesso!", tax.Date.ToString("MM/yy"));
        }

        if (newTaxes.Any())
        {
            _logger.LogInformation("Iniciando a criação de {Num} novos registros", newTaxes.Count);
            await _repository.InsertRows(newTaxes);
            _logger.LogInformation("Novos registros criados com sucesso");
        } else _logger.LogInformation("Não há novos registros a serem adicionados -> data atual: {Data}", DateTime.Now);
        _logger.LogInformation("Finalizada a atualização da base com sucesso");
    }
    private static AccumulatedValueDto CalculateAccumulatedSelic(int month, int startYear, IList<SelicAccumulatedDto> selicTaxes)
    {
        var (actualYear, actualMonth) = (DateTime.Now.Year, DateTime.Now.Month);
        if (startYear > actualYear) throw new ArgumentException("Não é possível calcular selic em data futura");
        var actualSelic = selicTaxes.FirstOrDefault(s => s.Date.Month == month && s.Date.Year == startYear);
        if (actualSelic == null && month != 12)
            throw new ArgumentException($"Taxa selic não encontrada para mes {month} e ano {startYear}");
        var acc = decimal.Zero;
        for (var year = startYear; year <= actualYear; year++)
        {
            if (year != startYear) month = 1;
            for (var m = month; m <= 12; m++)
            {
                if (year == actualYear && m > DateTime.Now.Month) break;
                if (year == actualYear && m == actualMonth && DateTime.Now.Day != DateTime.DaysInMonth(year, m))
                {
                    acc += 1;
                    break;
                }

                var nextSelic = selicTaxes.FirstOrDefault(s => s.Date.Month == m && s.Date.Year == year);
                if (nextSelic == null)
                    throw new ArgumentException($"Taxa selic não encontrada para mes {m} e ano {year}");
                acc += nextSelic.Value;
            }
        }

        return new AccumulatedValueDto(new DateTime(startYear, month, DateTime.DaysInMonth(startYear, month)), acc);
    }
}