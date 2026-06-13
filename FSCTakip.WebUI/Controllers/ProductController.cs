using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FSCTakip.WebUI.Controllers
{
    public class ProductController : BaseController
    {
        public ProductController(AppDbContext context) : base(context) { }

        #region Torba Tipleri
        public async Task<IActionResult> BagTypes()
        {
            var list = await _context.BagTypes.OrderBy(b => b.Name).ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBagType(BagType model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["Error"] = "TORBA TİPİ ADI ZORUNLUDUR.";
                return RedirectToAction("BagTypes");
            }

            if (model.Id == 0)
            {
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = "SYSTEM";
                model.IsActive = true;
                _context.BagTypes.Add(model);
            }
            else
            {
                var existing = await _context.BagTypes.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name.ToUpper();
                    existing.Code = model.Code?.ToUpper() ?? "";
                    existing.Description = model.Description?.ToUpper() ?? "";
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = "SYSTEM";
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("BagTypes");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBagTypeStatus(int id)
        {
            var item = await _context.BagTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "KAYIT BULUNAMADI." });

            item.IsActive = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = "SYSTEM";
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBagType(int id)
        {
            var item = await _context.BagTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            _context.BagTypes.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Torba tipi silindi." });
        }
        #endregion

        #region Ürün Grupları
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.ProductGroups.OrderBy(g => g.GroupName).ToListAsync();
            return View(groups);
        }

        // POST /Product/QuickAddGroup — inline hızlı ürün grubu ekleme
        [HttpPost]
        public async Task<IActionResult> QuickAddGroup(string GroupName, int? GroupCode)
        {
            if (string.IsNullOrWhiteSpace(GroupName))
                return Json(new { success = false, message = "Grup adı zorunludur." });
            var code = GroupCode ?? (await _context.ProductGroups.MaxAsync(g => (int?)g.GroupCode) ?? 0) + 1;
            var g = new ProductGroup
            {
                GroupName   = GroupName.Trim().ToUpper(new System.Globalization.CultureInfo("tr-TR")),
                GroupCode   = code,
                RangeStart  = code * 1000,
                RangeEnd    = code * 1000 + 999,
                IsActive    = true,
                CreatedDate = DateTime.Now,
                CreatedBy   = User.Identity?.Name ?? "System"
            };
            _context.ProductGroups.Add(g);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = g.Id, text = g.GroupName });
        }

        [HttpPost]
        public async Task<IActionResult> SaveProductGroup(ProductGroup model)
        {
            if (string.IsNullOrWhiteSpace(model.GroupName))
            {
                TempData["Error"] = "GRUP ADI ZORUNLUDUR.";
                return RedirectToAction("Groups");
            }

            if (model.Id == 0)
            {
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = "SYSTEM";
                model.IsActive = true;
                _context.ProductGroups.Add(model);
            }
            else
            {
                var existing = await _context.ProductGroups.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.GroupCode = model.GroupCode;
                    existing.GroupName = model.GroupName.ToUpper();
                    existing.RangeStart = model.RangeStart;
                    existing.RangeEnd = model.RangeEnd;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = "SYSTEM";
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Groups");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleGroupStatus(int id)
        {
            var group = await _context.ProductGroups.FindAsync(id);
            if (group == null) return Json(new { success = false, message = "GRUP BULUNAMADI." });

            group.IsActive = !group.IsActive;
            group.UpdatedDate = DateTime.Now;
            group.UpdatedBy = "SYSTEM";
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = group.IsActive });
        }
        #endregion

        public async Task<IActionResult> ExportBagTypes()
        {
            var data = await _context.BagTypes.OrderBy(b => b.Name).Select(b => new { b.Id, b.Name, b.Code, Durum = b.IsActive ? "AKTİF" : "PASİF" }).ToListAsync();
            return ExportToExcel(data, "TorbaTipleri");
        }

        // GET /Product/ExportProductGroups
        public async Task<IActionResult> ExportProductGroups()
        {
            var data = await _context.ProductGroups.OrderBy(g => g.GroupCode).Select(g => new {
                GrupKodu   = g.GroupCode,
                GrupAdi    = g.GroupName,
                AralikBas  = g.RangeStart,
                AralikBit  = g.RangeEnd,
                Durum      = g.IsActive ? "AKTİF" : "PASİF"
            }).ToListAsync();
            return ExportToExcel(data, "UrunGruplari");
        }

        // POST /Product/DeleteProductGroup
        [HttpPost]
        public async Task<IActionResult> DeleteProductGroup(int id)
        {
            var item = await _context.ProductGroups.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            var used = await _context.Products.AnyAsync(p => p.ProductGroupId == id);
            if (used) return Json(new { success = false, message = "Bu grup ürünlerde kullanılmaktadır." });
            _context.ProductGroups.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Grup silindi." });
        }
    }
}