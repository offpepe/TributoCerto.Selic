using TributoCerto.Selic.Service;
using TributoCerto.Selic.Service.Interfaces;

namespace TributoCerto.Selic.Jobs;

public class UpdateSelicTableJob : JobService
{
    private readonly ISelicService _service;
    private readonly ILogger<UpdateSelicTableJob> _logger;
    public UpdateSelicTableJob(IScheduleConfig<UpdateSelicTableJob> config, ILogger<UpdateSelicTableJob> logger, ISelicService service) : base(config.CronExpression, config.TimeZoneInfo)
    {
        _logger = logger;
        _service = service;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Jobs agendados para {DataJob}", Expression.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo));
        return base.StartAsync(cancellationToken);
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando a atualização da base da taxa selic");
        await _service.SeedDatabaseWithSelicTaxes(DateTime.Now.Year - 2013);
        _logger.LogInformation("Atualização concluída");
        await base.DoWork(cancellationToken);
    }
}