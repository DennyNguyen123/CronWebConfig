using Cronos;

public abstract class CronJobService : IHostedService, IDisposable
{
    private System.Timers.Timer? timer;

    private TimeZoneInfo timeZoneInfo;

    private CronFormat cronFormat;

    private CronExpression cronExpression;

    public CancellationTokenSource? _cancellationTokenSource;

    public bool State { get; set; }

    protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, CronFormat cronFormat = CronFormat.Standard)
    {
        this.timeZoneInfo = timeZoneInfo;
        this.cronFormat = cronFormat;
        this.cronExpression = CronExpression.Parse(cronExpression, this.cronFormat);
    }

    public virtual async Task Reconfig(string cronExpression, CronFormat cronFormat, TimeZoneInfo timeZoneInfo)
    {
        this.timeZoneInfo = timeZoneInfo;
        this.cronFormat = cronFormat;
        this.cronExpression = CronExpression.Parse(cronExpression, this.cronFormat);
        timer?.Stop();
        timer?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        await ScheduleJob(_cancellationTokenSource.Token);

    }

    public virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        DateTimeOffset? next = cronExpression.GetNextOccurrence(DateTimeOffset.Now, timeZoneInfo);
        if (next.HasValue)
        {
            TimeSpan delay = next.Value - DateTimeOffset.Now;
            if (delay.TotalMilliseconds <= 0.0)
            {
                await ScheduleJob(cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
            }

            timer = new System.Timers.Timer(delay.TotalMilliseconds);
            timer.Elapsed += async delegate
            {
                timer.Dispose();
                if (!cancellationToken.IsCancellationRequested)
                {
                    await DoWork(cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
                }
            };
            timer.Start();
        }

        await Task.CompletedTask.ConfigureAwait(continueOnCapturedContext: true);
    }

    public virtual async Task RunManual()
    {
        CancellationTokenSource cancelsource = new CancellationTokenSource();
        await DoWork(cancelsource.Token).ConfigureAwait(continueOnCapturedContext: true);
        cancelsource.Cancel(true);
    }
    public virtual async Task DoWork(CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
    }

    public void Dispose()
    {
        timer?.Dispose();
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_cancellationTokenSource == null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        if (!this.State)
        {
            // await StartAsync(_cancellationTokenSource.Token);
            await ScheduleJob(cancellationToken).ConfigureAwait(continueOnCapturedContext: true);
            State = true;
        }
    }


    public virtual async Task StopAsync()
    {
        if (this.State)
        {
            var token = _cancellationTokenSource?.Token == null ? new CancellationToken() : _cancellationTokenSource.Token;
            await StopAsync(token);
        }
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Stop();
        await Task.CompletedTask.ConfigureAwait(continueOnCapturedContext: true);
        this.State = false;
    }

}

public interface ICronConfiguration<T>
{
    string CronExpression { get; set; }

    TimeZoneInfo TimeZoneInfo { get; set; }

    CronFormat CronFormat { get; set; }
}

public class CronConfiguration<T> : ICronConfiguration<T>
{
    public string CronExpression { get; set; } = "* * * * *";

    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

    public CronFormat CronFormat { get; set; } = CronFormat.Standard;

}

public static class Startup
{
    public static IServiceCollection ApplyResulation<T>(this IServiceCollection services, Action<ICronConfiguration<T>> action) where T : CronJobService
    {
        CronConfiguration<T> cronConfiguration = new CronConfiguration<T>();
        action(cronConfiguration);
        services.AddSingleton((ICronConfiguration<T>)cronConfiguration);
        services.AddHostedService<T>();
        return services;
    }
}


