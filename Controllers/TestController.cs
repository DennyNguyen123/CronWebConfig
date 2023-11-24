using Cronos;
using Microsoft.AspNetCore.Mvc;

public class TestController : Controller
{
    private MyCronJob1 _service;
    public TestController(IServiceProvider serviceProvider)
    {
        // _cronlog = cronlog;
        _service = serviceProvider.GetServices<IHostedService>().OfType<MyCronJob1>().Single();
    }
    public IActionResult Index()
    {
        ViewBag.CronState = _service._cancellationTokenSource.IsCancellationRequested;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Stop()
    {
        _service._cancellationTokenSource.Cancel(true);
        // _service.StopAsync(new CancellationToken());
        return View("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Start()
    {
        await _service.StartAsync(new CancellationToken());
        ViewBag.CronState = _service._cancellationTokenSource.IsCancellationRequested;
        return View("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Run()
    {
        await _service.RunManual();
        return View("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Reconfig([FromQuery] string format)
    {
        await _service.StopAsync(new CancellationToken());
        _service.Reconfig(format, CronFormat.IncludeSeconds, TimeZoneInfo.Local);
        await _service.StartAsync(new CancellationToken());
        return View("Index");
    }

    public IActionResult Check()
    {

        var rs = _service._cancellationTokenSource.IsCancellationRequested;
        return View("Index");
    }


}