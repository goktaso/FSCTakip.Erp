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
    // Güvenlik: CSRF derinliği için SameSite=Lax; HTTPS'te Secure (intranet HTTP'de
    // çerezi kırmamak için SameAsRequest — https ise Secure, http ise değil).
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Antiforgery çerezi Session çerezinden AYRI bir CookieBuilder kullanır — açıkça
// ayarlanmazsa framework varsayılanına kalır. Intranet kurulumu düz HTTP + IP
// adresiyle (https değil, DNS adı değil) erişildiğinde varsayılan davranış token
// uyuşmazlığına ve POST'larda 400'e yol açabiliyordu (VM testi 2026-07-18 —
// Şifre Değiştir formunda görüldü). Session ile aynı politika verildi.
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// PermissionService ve HttpContextAccessor kaydı
builder.Services.AddScoped<PermissionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<ILicenseService, LicenseService>();
builder.Services.AddSingleton<ICompanyBrandingService, CompanyBrandingService>();
builder.Services.AddHttpClient<IUpdateCheckService, UpdateCheckService>();
builder.Services.AddHostedService<EtlBackgroundService>();

// DbContext kaydı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Middleware (Ara Katman) Yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // Intranet kurulumları HTTP üzerinden yayındadır (bkz. ThirdPartyKurulum FAZ 3.2).
    // Böyle bir sunucuda HTTPS'e yönlendirmek kullanıcıyı ERR_SSL_PROTOCOL_ERROR'a düşürür;
    // HSTS ise tarayıcıyı https'e kalıcı kilitler. Kurulum scripti bu bayrağı false yazar.
    if (app.Configuration.GetValue("Security:HttpsRedirection", true))
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
}
app.UseStaticFiles();

// 404/403 gibi durum kodlarını kurumsal Türkçe sayfaya yönlendir (statik dosyalardan sonra
// ki gerçek 404'lar — eksik css/js — bu sayfaya düşmesin).
app.UseStatusCodePagesWithReExecute("/Home/HttpError/{0}");

// Proxy / CDN içerik dönüşümünü engelle — charset bozulmasını önler
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("Cache-Control", "no-transform");
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    await next();
});

app.UseRouting();

// SIRA ÇOK ÖNEMLİ: Authentication mutlaka Authorization'dan ÖNCE gelmeli
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Şema güncelle. DB yoksa Migrate() oluşturur — ancak sunucu varsayılan collation'ıyla;
// bu yüzden kurulum scripti FscErpDb'yi Turkish_CI_AS ile önceden yaratır (bkz. ThirdPartyKurulum FAZ 2).
// DBA'sı olan müşteride Database:AutoMigrate=false ile kapatılıp migration.sql yolu kullanılabilir.
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
    if (pending.Count > 0)
    {
        Log.Information("Bekleyen {Count} migration uygulanıyor...", pending.Count);
        await ctx.Database.MigrateAsync();
        Log.Information("Veritabanı şeması güncellendi.");
    }
}

// Referans veri (admin + şirket kaydı + FSC tipleri) her zaman; örnek işlem verisi
// yalnız Seed:DemoData=true iken (geliştirme). Müşteri kurulumunda kurulum scripti
// bu bayrağı false yazar → sistem boş gelir, sahte kayıt görünmez.
await DbSeeder.SeedAsync(app.Services, app.Configuration.GetValue("Seed:DemoData", false));

app.Run();