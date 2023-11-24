
using Cronos;
public class MyCronJob2 : CronJobService
{
    private readonly ILogger<MyCronJob2> _logger;

    public MyCronJob2(ICronConfiguration<MyCronJob2> config, ILogger<MyCronJob2> logger)
        : base(config.CronExpression, config.TimeZoneInfo, config.CronFormat)
    {
        _logger = logger;

    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start job 2");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync()
    {
        _logger.LogInformation("Stop job 2");
        return base.StopAsync();
    }


    public override async Task Reconfig(string cronExpression, CronFormat cronFormat, TimeZoneInfo timeZoneInfo)
    {
        _logger.LogInformation("Reconfig job 2");
        await base.Reconfig(cronExpression, cronFormat, timeZoneInfo);
    }

    public override async Task RunManual()
    {
        _logger.LogInformation("Run manual job 2");
        await base.RunManual();
    }
    public override async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Job 2 start");
        // await Task.Delay(3000);
        // _logger.LogInformation($"Work end {DateTime.Now}");

        await base.DoWork(cancellationToken);
    }
}