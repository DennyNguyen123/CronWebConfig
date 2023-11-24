
using Cronos;

public class MyCronJob1 : CronJobService
{
    private readonly ILogger<MyCronJob1> _logger;

    public MyCronJob1(ICronConfiguration<MyCronJob1> config, ILogger<MyCronJob1> logger)
        : base(config.CronExpression, config.TimeZoneInfo, config.CronFormat)
    {
        _logger = logger;

    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start job");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync()
    {
        _logger.LogInformation("Stop job");
        return base.StopAsync();
    }


    public override async Task Reconfig(string cronExpression, CronFormat cronFormat, TimeZoneInfo timeZoneInfo)
    {
        _logger.LogInformation("Reconfig");
        await base.Reconfig(cronExpression, cronFormat, timeZoneInfo);
    }

    public override async Task RunManual()
    {
        _logger.LogInformation("Run manual");
        await base.RunManual();
    }
    public override async Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Work start {DateTime.Now}");
        await Task.Delay(3000);
        _logger.LogInformation($"Work end {DateTime.Now}");

        await base.DoWork(cancellationToken);
    }
}