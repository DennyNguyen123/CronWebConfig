
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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stop job");
        return base.StopAsync(cancellationToken);
    }

    public override void Reconfig(string cronExpression, CronFormat cronFormat, TimeZoneInfo timeZoneInfo)
    {
        _logger.LogInformation("Reconfig");
        base.Reconfig(cronExpression, cronFormat, timeZoneInfo);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Work {DateTime.Now}");
        return base.DoWork(cancellationToken);
    }
}