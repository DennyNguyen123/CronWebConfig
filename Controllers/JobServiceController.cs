using System.ComponentModel.DataAnnotations;
using Cronos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class JobServiceController : Controller
{
    private readonly List<CronJobService> _lstservice;
    private readonly ILogger<JobServiceController> _logger;
    public JobServiceController(IServiceProvider serviceProvider, ILogger<JobServiceController> logger)
    {
        _logger = logger;
        // _cronlog = cronlog;
        var services = serviceProvider.GetServices<IHostedService>().OfType<CronJobService>().ToList();
        _lstservice = services;


        // _service = serviceProvider.GetServices<IHostedService>().OfType<MyCronJob1>().Single();
    }
    public IActionResult Index()
    {
        // ViewBag.CronState = _service.State;
        // return View(_lstservice);
        // _logger.LogInformation(_lstservice.Count().ToString());
        // Response.Headers.Add("Refresh", "1");
        return View(_lstservice);
    }

    public async Task<IActionResult> Stop(string servicename)
    {
        var service = _lstservice.Where(o => o.JobName == servicename).Single();
        await service.StopAsync();
        return RedirectToAction("Index", _lstservice);
    }



    public async Task<IActionResult> Start(string servicename)
    {
        var service = _lstservice.Where(o => o.JobName == servicename).Single();
        await service.StartAsync(new CancellationToken());
        return RedirectToAction("Index", _lstservice);
    }

    public async Task<IActionResult> RunManual(string servicename)
    {
        var service = _lstservice.Where(o => o.JobName == servicename).Single();
        await service.RunManual();
        return RedirectToAction("Index", _lstservice);
    }


    [HttpGet]
    public IActionResult GetReconfig(string servicename)
    {
        var service = _lstservice.Where(o => o.JobName == servicename).Single();
        var reconfig = new ReconfigModel();
        reconfig.ServiceName = service.JobName;
        reconfig.TimeZone = service.timeZoneInfo == TimeZoneInfo.Local ? "Local" : "UTC";
        reconfig.Expression = service.cronExpressionstring;
        reconfig.JobDesc = service.JobDescription;
        reconfig.CronFormat = service.cronFormat;
        reconfig.IsRunOnStartup = service.IsRunOnStartup;
        return PartialView("_ReconfigPartial", reconfig);
    }

    public async Task<IActionResult> Reconfig(ReconfigModel reconfig)
    {

        var service = _lstservice.FirstOrDefault(o => o.JobName == reconfig.ServiceName);
        await service.Reconfig(reconfig.Expression, reconfig.CronFormat.ToString(), reconfig.TimeZone, reconfig.JobDesc);


        return RedirectToAction("Index", _lstservice);
    }


}

public class ReconfigModel
{
    [Required]
    public string ServiceName { get; set; }
    [Required]
    public string Expression { get; set; }
    public CronFormat CronFormat { get; set; }
    public string TimeZone { get; set; }
    public string? JobDesc { get; set; }
    public bool IsRunOnStartup { get; set; }
}