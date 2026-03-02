using Microsoft.EntityFrameworkCore;
using FSCTakip.DataAc.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Gerekli Servisleri Ekle (Hata buradaydý)
builder.Services.AddControllersWithViews(); // MVC için þart
builder.Services.AddAuthorization();        // Aldýðýn hatayý çözen satýr
builder.Services.AddAuthentication();       // Authorization ile ayrýlmaz ikilidir

// DbContext kaydý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Middleware (Ara Katman) Yapýlandýrmasý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// SIRA ÇOK ÖNEMLÝ: Authentication mutlaka Authorization'dan ÖNCE gelmeli
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();