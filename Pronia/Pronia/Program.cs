using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    );
builder.Services.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();
builder.Services.AddScoped<LayoutService>();
var app = builder.Build();
app.UseSession();
app.UseAuthorization();
app.UseRouting();
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});
app.MapControllerRoute(

    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );

app.Run();
