using Proj.Configurations;
using Proj.Data;

var builder = WebApplication.CreateBuilder(args);


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

try
{
    await app.RunAsync();
}
catch (Exception e) when (e is not HostAbortedException)
{
    await app.StopAsync();
}
finally
{
    await app.DisposeAsync();
}

public partial class Program
{
}