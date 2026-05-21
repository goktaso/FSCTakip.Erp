using FSCTakip.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using FSCTakip.DataAccess.Data;
namespace FSCTakip.WebUI.Controllers
{
    public class PaperController : BaseController
    {
        public PaperController(AppDbContext context) : base(context) { }

        // ── QUICK-ADD ENDPOINTS (inline modal kayıtları) ────────────────

        [HttpPost] public async Task<IActionResult> QuickAddFscType(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name)) return Json(new { success=false, message="Ad zorunludur." });
            var e = new FSCTakip.Core.Entities.FscType { Name=Name.Trim().ToUpper(), IsActive=true };
            _context.FscTypes.Add(e); await _context.SaveChangesAsync();
            return Json(new { success=true, id=e.Id, text=e.Name });
        }

        [HttpPost] public async Task<IActionResult> QuickAddColor(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name)) return Json(new { success=false, message="Ad zorunludur." });
            var e = new PaperColor { Name=Name.Trim().ToUpper(), IsActive=true };
            _context.PaperColors.Add(e); await _context.SaveChangesAsync();
            return Json(new { success=true, id=e.Id, text=e.Name });
        }

        [HttpPost] public async Task<IActionResult> QuickAddPaperType(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name)) return Json(new { success=false, message="Ad zorunludur." });
            var e = new PaperType { Name=Name.Trim().ToUpper(), IsActive=true };
            _context.PaperTypes.Add(e); await _context.SaveChangesAsync();
            return Json(new { success=true, id=e.Id, text=e.Name });
        }

        [HttpPost] public async Task<IActionResult> QuickAddWeight(decimal Value, string? Unit)
        {
            if (Value <= 0) return Json(new { success=false, message="Gramaj değeri 0'dan büyük olmalıdır." });
            var u = string.IsNullOrWhiteSpace(Unit) ? "gr/m²" : Unit.Trim();
            var e = new PaperWeight { Value=Value, Unit=u, IsActive=true };
            _context.PaperWeights.Add(e); await _context.SaveChangesAsync();
            return Json(new { success=true, id=e.Id, text=$"{Value} {u}" });
        }

        [HttpPost] public async Task<IActionResult> QuickAddWidth(string Code, decimal Value)
        {
            if (string.IsNullOrWhiteSpace(Code) || Value <= 0) return Json(new { success=false, message="Kod ve değer zorunludur." });
            var e = new PaperWidth { Code=Code.Trim().ToUpper(), Value=Value, Unit="mm", IsActive=true };
            _context.PaperWidths.Add(e); await _context.SaveChangesAsync();
            return Json(new { success=true, id=e.Id, text=$"{Code.ToUpper()} ({Value} mm)" });
        }

        // --- LİSTELEME METOTLARI ---
        public async Task<IActionResult> Types() => View(await _context.PaperTypes.ToListAsync());
        public async Task<IActionResult> Colors() => View(await _context.PaperColors.ToListAsync());
        public async Task<IActionResult> FscTypes() => View(await _context.FscTypes.ToListAsync());
        public async Task<IActionResult> Widths() => View(await _context.PaperWidths.ToListAsync());
        public async Task<IActionResult> Weights() => View(await _context.PaperWeights.ToListAsync());

        // --- KAYDETME METOTLARI ---

        [HttpPost]
        public async Task<IActionResult> SaveType(PaperType model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperTypes.Add(model); }
            else _context.PaperTypes.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Types));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteType(int id)
        {
            var item = await _context.PaperTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.PaperTypes.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Kağıt tipi silindi." });
        }

        // --- [YENİ EKLENDİ] - Kağıt Tipi Durum Değiştirme Metodu ---
        [HttpPost]
        public async Task<IActionResult> ToggleTypeStatus(int id)
        {
            var item = await _context.PaperTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive; // Durumu tersine çevir
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> SaveColor(PaperColor model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperColors.Add(model); }
            else _context.PaperColors.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Colors));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteColor(int id)
        {
            var item = await _context.PaperColors.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.PaperColors.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Kağıt rengi silindi." });
        }

        // --- [YENİ EKLENDİ] - Renk Durum Değiştirme Metodu ---
        [HttpPost]
        public async Task<IActionResult> ToggleColorStatus(int id)
        {
            var item = await _context.PaperColors.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive; // Durumu tersine çevir
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> SaveFscType(FSCTakip.Core.Entities.FscType model)
        {
            if (model.Id == 0)
            {
                model.IsActive = true;
                _context.FscTypes.Add(model);
            }
            else
            {
                _context.FscTypes.Update(model);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(FscTypes));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFscType(int id)
        {
            var item = await _context.FscTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.FscTypes.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "FSC tipi silindi." });
        }

        // --- [YENİ EKLENDİ] - FSC Tipi Durum Değiştirme Metodu ---
        [HttpPost]
        public async Task<IActionResult> ToggleFscStatus(int id)
        {
            var item = await _context.FscTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWidth(PaperWidth model)
        {
            if (model.Value <= 0)
            {
                TempData["Error"] = "Geçerli bir genişlik değeri giriniz.";
                return RedirectToAction(nameof(Widths));
            }

            if (model.Id == 0)
            {
                model.IsActive = true;
                // BaseEntity'den geliyorsa bu alanları dolduruyoruz
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = User.Identity?.Name ?? "System";
                _context.PaperWidths.Add(model);
            }
            else
            {
                var existing = await _context.PaperWidths.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Value = model.Value;
                    existing.Code = model.Code;
                    existing.Unit = model.Unit;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = User.Identity?.Name ?? "System";
                    _context.PaperWidths.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Widths));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWidth(int id)
        {
            var item = await _context.PaperWidths.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.PaperWidths.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Kağıt eni başarıyla silindi." });
        }


        [HttpPost]
        public async Task<IActionResult> ToggleWidthStatus(int id)
        {
            var item = await _context.PaperWidths.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive; // Mevcut durumun tersini yap (True ise False, False ise True)
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWeight(PaperWeight model)
        {
            if (model.Id == 0) { model.IsActive = true; _context.PaperWeights.Add(model); }
            else _context.PaperWeights.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Weights));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWeight(int id)
        {
            var item = await _context.PaperWeights.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.PaperWeights.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Gramaj kaydı silindi." });
        }

        // --- [YENİ EKLENDİ] - Gramaj Durum Değiştirme Metodu ---
        [HttpPost]
        public async Task<IActionResult> ToggleWeightStatus(int id)
        {
            var item = await _context.PaperWeights.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            item.IsActive = !item.IsActive; // Durumu tersine çevir (True/False)
            ;

            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        // --- EXCEL EXPORT METOTLARI ---
        public async Task<IActionResult> ExportWeights() => ExportToExcel(await _context.PaperWeights.ToListAsync(), "Gramajlar");
        public async Task<IActionResult> ExportColors() => ExportToExcel(await _context.PaperColors.ToListAsync(), "Renkler");
        public async Task<IActionResult> ExportTypes() => ExportToExcel(await _context.PaperTypes.ToListAsync(), "KagitTipleri");
        public async Task<IActionResult> ExportWidths() => ExportToExcel(await _context.PaperWidths.ToListAsync(), "BobinEnleri");
        public async Task<IActionResult> ExportFscTypes() => ExportToExcel(await _context.FscTypes.ToListAsync(), "FSCTipleri");
    }
}