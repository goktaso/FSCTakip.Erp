using Microsoft.EntityFrameworkCore;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Data;
using FSCTakip.Business.Services;
using FSCTakip.WebUI.Filters;
using FSCTakip.WebUI.Binders;
using FSCTakip.WebUI.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/fsc-erp-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// 1. Gerekli Servisleri Ekle
var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LicenseFilter>();      // lisans kontrolü — auth'tan da önce
    options.Filters.Add<SessionAuthFilter>();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    // Decimal form alanlarını kültürden bağımsız bağla (tr-TR'de "6000.00" → 60000000 bug'ı)
    options.ModelBinderProviders.Insert(0, new InvariantDecimalModelBinderProvider());
});

// Development ortamında .cshtml değişiklikleri rebuild gerektirmez
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// PermissionService ve HttpContextAccessor kaydı
builder.Services.AddScoped<PermissionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<ILicenseService, LicenseService>();
builder.Services.AddHostedService<EtlBackgroundService>();

// DbContext kayd�
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Middleware (Ara Katman) Yap�land�rmas�
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection(); // Sadece production'da HTTPS'e yönlendir
}
app.UseStaticFiles();

// Proxy / CDN içerik dönüşümünü engelle — charset bozulmasını önler
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("Cache-Control", "no-transform");
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    await next();
});

app.UseRouting();

// SIRA �OK �NEML�: Authentication mutlaka Authorization'dan �NCE gelmeli
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Demo veri yükle (DB boşsa)
await DbSeeder.SeedAsync(app.Services);

app.Run();