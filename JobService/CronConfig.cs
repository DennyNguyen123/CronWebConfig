using Cronos;

public static class Startup
{
    public static IServiceCollection ApplyResulation<T>(this IServiceCollection services, Action<ICronConfiguration<T>> action) where T : CronJobService
    {
        CronConfiguration<T> cronConfiguration = new CronConfiguration<T>();
        action(cronConfiguration);
        cronConfiguration.IsFromConfig = false;
        services.AddSingleton((ICronConfiguration<T>)cronConfiguration);
        var host = services.AddHostedService<T>();

        return services;
    }

    public static IServiceCollection ApplyResulationByConfig<T>(this IServiceCollection services, string configpath = "") where T : CronJobService
    {
        var cronConfiguration = CronConfiguration<T>.Get(configpath);
        // action(cronConfiguration);
        cronConfiguration.IsFromConfig = true;
        cronConfiguration.ConfigPath = configpath;
        services.AddSingleton((ICronConfiguration<T>)cronConfiguration);
        var host = services.AddHostedService<T>();
        return services;
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

    public bool IsRunOnStartup { get; set; }
}

public class CronConfiguration<T> : ICronConfiguration<T>
{
    public bool IsRunOnStartup { get; set; } = true;
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
        var isrunonstart = config.JsonObj["CronJobs"][ServiceName]["IsRunOnStartup"];
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
        cronConfiguration.IsRunOnStartup = isrunonstart ?? true;
        return cronConfiguration;
    }


}

