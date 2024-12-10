using Proj.Configurations;
using Proj.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();

builder.ConfigureDataLayer().ConfigureIdentity();

builder.Services.AddControllersWithViews();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { controller = "Home", action = "Index" });
app.MapRazorPages();
app.UseStatusCodePages();

try
{
    await app.RunAsync();
    Log.Information("Stopped cleanly");
}
catch (Exception e) when (e is not HostAbortedException)
{
    Log.Fatal(e, "Host terminated unexpectedly");
    await app.StopAsync();
}
finally
{
    Log.CloseAndFlush();
    await app.DisposeAsync();
}

public partial class Program
{
}