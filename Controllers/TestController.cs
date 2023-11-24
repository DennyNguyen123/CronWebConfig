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
        ViewBag.CronState = _service.State;
        return View();
    }

    [HttpGet]
    public IActionResult Stop()
    {
        _service.StopAsync();
        // _service.StopAsync(new CancellationToken());
        ViewBag.CronState = _service.State;
        return View("Index");
    }

    public async Task<IActionResult> StopAll()
    {
        await _service.StopAsync();
        ViewBag.CronState = _service.State;
        return View("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Start()
    {
        await _service.StartAsync(new CancellationToken());
        ViewBag.CronState = _service.State;
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
        await _service.Reconfig(format, CronFormat.IncludeSeconds, TimeZoneInfo.Local);
        await _service.StartAsync(new CancellationToken());
        return View("Index");
    }

    public IActionResult Check()
    {

        var rs = _service._cancellationTokenSource?.IsCancellationRequested;
        return View("Index");
    }


}