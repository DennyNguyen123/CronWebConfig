using Serilog;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager config = builder.Configuration;
// Add services to the container.
builder.Services.AddControllersWithViews();

// builder.Services.ApplyResulation<MyCronJob1>(options =>
// {
//     options.TimeZoneInfo = TimeZoneInfo.Local;
//     options.CronExpression = @"*/7 * * * * *";
//     options.CronFormat = Cronos.CronFormat.IncludeSeconds;
// });

builder.Services.ApplyResulationByConfig<MyCronJob1>("cron.config.json", "MyCronJob");

// builder.Logging.ClearProviders();
// builder.Logging.AddEventLog();

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
