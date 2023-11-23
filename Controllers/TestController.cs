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
        return View();
    }

    [HttpGet]
    public IActionResult Stop()
    {
        _service.StopAsync(new CancellationToken());
        return Ok();
    }

    [HttpGet]
    public IActionResult Start()
    {
        _service.StartAsync(new CancellationToken());
        return Ok();
    }

    [HttpGet]
    public IActionResult Run()
    {
        _service.DoWork(new CancellationToken());
        return Ok();
    }


    [HttpGet]
    public async Task<IActionResult> Reconfig([FromQuery] string format)
    {
        await _service.StopAsync(new CancellationToken());
        _service.Reconfig(format, CronFormat.IncludeSeconds, TimeZoneInfo.Local);
        await _service.StartAsync(new CancellationToken());
        return Ok();
    }


}