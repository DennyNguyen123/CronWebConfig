
using Cronos;

public class MyCronJob1 : CronJobService
{
    private readonly ILogger<MyCronJob1> _logger;

    public MyCronJob1(ICronConfiguration<MyCronJob1> config, ILogger<MyCronJob1> logger)
        : base(config.CronExpression, config.TimeZoneInfo, config.CronFormat, config.JobDesc, config.IsFromConfig, config.ConfigPath, config.IsRunOnStartup)
    {
        _logger = logger;

    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (!IsRunOnStartup)
        {
            _logger.LogInformation("Start job 1");
        }
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync()
    {
        _logger.LogInformation("Stop job 1");
        return base.StopAsync();
    }


    public override Task Reconfig(string cronExpression, string cronformatstr, string timeZoneInfo, string? jobDescription, bool isStartOnStartup)
    {
        _logger.LogInformation("Reconfig job 1");
        return base.Reconfig(cronExpression, cronformatstr, timeZoneInfo, jobDescription, isStartOnStartup);
    }

    public override async Task RunManual()
    {
        _logger.LogInformation("Run manual job 1");
        await base.RunManual();
    }


    public override async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Job 1 run");
        // await Task.Delay(3000);
        // _logger.LogInformation($"Work end {DateTime.Now}");

        await base.DoWork(cancellationToken);
    }
}