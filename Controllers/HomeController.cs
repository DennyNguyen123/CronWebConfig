using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using webcrontab.Models;

namespace webcrontab.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    // private readonly CronConfiguration<CronJobService> _cron;
    public HomeController(ILogger<HomeController> logger)
    {
        // _cron = cron;
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Test");
        _logger.LogError("Test");
        _logger.LogInformation("Test");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Test()
    {
        // Console.WriteLine();
        return View();
    }
}
