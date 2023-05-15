using Cronos;

namespace TributoCerto.Selic.Jobs;
public abstract class JobService : IHostedService, IDisposable
{
    private System.Timers.Timer? _timer;
    protected readonly CronExpression Expression;
    protected readonly TimeZoneInfo TimeZoneInfo;

    protected JobService(string cronExpression, TimeZoneInfo timeZoneInfo)
    {
        Expression = CronExpression.Parse(cronExpression);
        TimeZoneInfo = timeZoneInfo;
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        await ScheduleJob(cancellationToken);
    }

    protected virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        var next = Expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo);
        if (next.HasValue)
        {
            var delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJob(cancellationToken);
            }

            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (_, _) =>
            {
                _timer.Dispose();
                _timer = null;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await DoWork(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken);
                }
            };
            _timer.Start();
        }

        await Task.CompletedTask;
    }

    public virtual async Task DoWork(CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        await Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public interface IScheduleConfig<T>
{
    string CronExpression { get; set; }
    TimeZoneInfo TimeZoneInfo { get; set; }
}

public class ScheduleConfig<T> : IScheduleConfig<T>
{
    public string CronExpression { get; set; } = string.Empty;
    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;
}

public static class ScheduledServiceExtensions
{
    public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options)
        where T : JobService
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options), "Configure o agendador de tarefas.");
        }

        var config = new ScheduleConfig<T>();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression),
                "Cron Expression não pode ser vazia");
        }

        services.AddSingleton<IScheduleConfig<T>>(config);
        services.AddHostedService<T>();
        return services;
    }
}