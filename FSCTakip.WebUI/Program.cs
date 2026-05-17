using Microsoft.EntityFrameworkCore;
using FSCTakip.DataAccess.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Gerekli Servisleri Ekle
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DbContext kayd�
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Middleware (Ara Katman) Yap�land�rmas�
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// SIRA �OK �NEML�: Authentication mutlaka Authorization'dan �NCE gelmeli
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();