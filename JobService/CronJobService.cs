using Cronos;
public abstract class CronJobService : IHostedService, IDisposable
{
    private System.Timers.Timer? timer;

    public TimeZoneInfo timeZoneInfo;

    public CronFormat cronFormat;

    public CronExpression cronExpression;

    public string cronExpressionstring { get; set; }

    private CancellationTokenSource? _cancellationTokenSource;

    public string JobName { get => this.GetType().Name; }
    public string? JobDescription { get; set; }

    public bool IsFromConfig { get; set; }

    public string? ConfigPath { get; set; }

    public bool State { get; set; }
    public string StateString { get => State ? "Running" : "Stopped"; }

    public DateTimeOffset? next;

    protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, CronFormat cronFormat, string? jobDescription, bool isFromConfig, string? configpath)
    {
        this.ConfigPath = configpath;
        this.IsFromConfig = isFromConfig;
        this.JobDescription = jobDescription;
        this.timeZoneInfo = timeZoneInfo;
        this.cronFormat = cronFormat;
        this.cronExpressionstring = cronExpression;
        this.cronExpression = CronExpression.Parse(cronExpression, this.cronFormat);
    }


    private void WriteConfig(string expression, string timeZone, string cronformat, string? jobdesc)
    {
        var config = new AppConfig(this.ConfigPath ?? "");
        config.JsonObj["CronJobs"][JobName]["CronExpression"] = expression;
        config.JsonObj["CronJobs"][JobName]["TimeZoneInfo"] = timeZone;
        config.JsonObj["CronJobs"][JobName]["CronFormat"] = cronformat;
        config.JsonObj["CronJobs"][JobName]["CronFormat"] = jobdesc;
        config.SaveToFile();
    }
    public virtual async Task Reconfig(string cronExpression, string cronformatstr, string timeZoneInfo, string? jobDescription)
    {
        if (this.IsFromConfig)
        {
            timer?.Stop();
            timer?.Dispose();

            var tz = timeZoneInfo == "UTC" ? TimeZoneInfo.Utc : TimeZoneInfo.Local;
            this.timeZoneInfo = tz;

            Enum.TryParse(cronformatstr, true, out CronFormat cronFormat);
            this.cronFormat = cronFormat;
            this.cronExpression = CronExpression.Parse(cronExpression, this.cronFormat);
            this.cronExpressionstring = cronExpression;
            if (State)
            {
                await StopAsync();
            }

            this.WriteConfig(cronExpression, timeZoneInfo, cronformatstr, jobDescription);

            _cancellationTokenSource = new CancellationTokenSource();

            if (!State)
            {
                await StartAsync(_cancellationTokenSource.Token);
            }
        }
        else
        {
            throw new Exception("Only support for config type");
        }
        await Task.CompletedTask.ConfigureAwait(continueOnCapturedContext: true);
    }

    public virtual async Task ScheduleJob(CancellationToken cancellationToken)
    {
        next = cronExpression.GetNextOccurrence(DateTimeOffset.Now, timeZoneInfo);
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

            await ScheduleJob(_cancellationTokenSource.Token).ConfigureAwait(continueOnCapturedContext: true);
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
    public string? JobDesc { get; set; }

    public bool IsFromConfig { get; set; }

    public string? ConfigPath { get; set; }
}

public class CronConfiguration<T> : ICronConfiguration<T>
{
    public string CronExpression { get; set; } = "* * * * *";

    public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

    public CronFormat CronFormat { get; set; } = CronFormat.Standard;

    public static string ServiceName { get => typeof(T).Name; }

    public string? JobDesc { get; set; }

    public bool IsFromConfig { get; set; }
    public string? ConfigPath { get; set; }

    public static CronConfiguration<T> Get(string configpath = "")
    {
        // var config = Utils.GetAppSetting(configpath);
        var config = new AppConfig(configpath);

        var expression = config.JsonObj["CronJobs"][ServiceName]["CronExpression"].ToString();
        var tz = config.JsonObj["CronJobs"][ServiceName]["TimeZoneInfo"].ToString();
        var cf = config.JsonObj["CronJobs"][ServiceName]["CronFormat"].ToString();
        var jobdesc = config.JsonObj["CronJobs"][ServiceName]["JobDesc"].ToString();
        TimeZoneInfo timeZone;
        CronFormat cronFormat;

        if (string.IsNullOrEmpty(expression)
        || string.IsNullOrEmpty(tz)
        || string.IsNullOrEmpty(cf)
        )
        {
            throw new Exception($"Missing config for {ServiceName}");
        }

        if (tz == "Local")
        {
            timeZone = TimeZoneInfo.Local;
        }
        else if (tz == "UTC")
        {
            timeZone = TimeZoneInfo.Utc;
        }
        else
        {
            throw new Exception($"Not found TimeZone {tz}");
        }

        if (!Enum.TryParse(cf, true, out cronFormat))
        {
            throw new Exception($"Not found CronFormat {cf}");
        }



        CronConfiguration<T> cronConfiguration = new CronConfiguration<T>();
        cronConfiguration.CronExpression = expression;
        cronConfiguration.TimeZoneInfo = timeZone;
        cronConfiguration.CronFormat = cronFormat;
        cronConfiguration.JobDesc = jobdesc;
        return cronConfiguration;
    }


}

public static class Startup
{
    public static IServiceCollection ApplyResulation<T>(this IServiceCollection services, Action<ICronConfiguration<T>> action) where T : CronJobService
    {
        CronConfiguration<T> cronConfiguration = new CronConfiguration<T>();
        action(cronConfiguration);
        cronConfiguration.IsFromConfig = false;
        services.AddSingleton((ICronConfiguration<T>)cronConfiguration);
        services.AddHostedService<T>();
        return services;
    }

    public static IServiceCollection ApplyResulationByConfig<T>(this IServiceCollection services, string configpath = "") where T : CronJobService
    {

        var cronConfiguration = CronConfiguration<T>.Get(configpath);
        // action(cronConfiguration);
        cronConfiguration.IsFromConfig = true;
        cronConfiguration.ConfigPath = configpath;
        services.AddSingleton((ICronConfiguration<T>)cronConfiguration);
        services.AddHostedService<T>();

        return services;
    }
}


