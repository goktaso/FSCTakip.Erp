using FSCTakip.DataAccess.Data; // AppDbContext'in bulunduğu namespace (Kendi projene göre kontrol et)
using FSCTakip.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // AnyAsync ve SaveChangesAsync için gerekli

namespace FSC_ERP.Controllers
{
    public class SettingsController : Controller
    {
        // 1. ADIM: DbContext Tanımlama
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        // --- View Actionları ---
        public IActionResult Machines() { ViewData["Title"] = "Makine Tanımlamaları"; return View(); }
        public IActionResult ProductGroups() { ViewData["Title"] = "Ürün Grupları"; return View(); }
        public IActionResult BagTypes() { ViewData["Title"] = "Torba Tipleri"; return View(); }
        public IActionResult PaperTypes() { ViewData["Title"] = "Kağıt Tipleri"; return View(); }
        public IActionResult PaperColors() { ViewData["Title"] = "Kağıt Renkleri"; return View(); }
        public IActionResult FscTypes() { ViewData["Title"] = "FSC Sertifika Tipleri"; return View(); }
        public IActionResult BobinEnleri() { ViewData["Title"] = "Bobin En Tanımları"; return View(); }
        public IActionResult Grammages() { ViewData["Title"] = "Gramaj Tanımları"; return View(); }


        // 2. ADIM: Dinamik QuickSave Metodu
        [HttpPost]
        public async Task<IActionResult> QuickSave(string name, string type)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    return Json(new { success = false, message = "Ad alanı boş olamaz." });

                name = name.Trim().ToUpper();
                object? entity = null;

                // Gelen 'type' parametresine göre ilgili tabloya kayıt atıyoruz
                switch (type)
                {
                    case "PaperColor":
                        if (await _context.PaperColors.AnyAsync(x => x.Name == name))
                            return Json(new { success = false, message = "Bu renk zaten mevcut." });
                        entity = new PaperColor { Name = name, IsActive = true };
                        _context.PaperColors.Add((PaperColor)entity);
                        break;

                    case "PaperType":
                        entity = new PaperType { Name = name, IsActive = true };
                        _context.PaperTypes.Add((PaperType)entity);
                        break;

                    case "Unit":
                        entity = new Unit { Name = name, IsActive = true };
                        _context.Units.Add((Unit)entity);
                        break;

                    case "ProductGroup":
                        entity = new ProductGroup { GroupName = name, IsActive = true };
                        _context.ProductGroups.Add((ProductGroup)entity);
                        break;

                    // İhtiyacın olan diğer caseleri buraya ekleyebilirsin (Supplier, FscType vb.)

                    default:
                        return Json(new { success = false, message = "Geçersiz tip: " + type });
                }

                await _context.SaveChangesAsync();

                // Reflection kullanarak eklenen entity'nin ID'sini alıyoruz
                var idProperty = entity.GetType().GetProperty("Id");
                int newId = (int)(idProperty?.GetValue(entity) ?? 0);

                return Json(new { success = true, id = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}