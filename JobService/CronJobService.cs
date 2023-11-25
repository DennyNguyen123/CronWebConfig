using Cronos;
using Newtonsoft.Json;
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
    protected bool IsRunOnStartup { get; set; }

    protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, CronFormat cronFormat, string? jobDescription, bool isFromConfig, string? configpath, bool isStartOnStartup)
    {
        this.IsRunOnStartup = isStartOnStartup;
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
        if (!IsRunOnStartup)
        {
            IsRunOnStartup = true;
            return;
        }

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



