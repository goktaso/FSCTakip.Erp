using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSCTakip.WebUI.Controllers
{
    public class ProductsController : BaseController
    {
        // Hata veren satırı bununla değiştir:
        public ProductsController(FSCTakip.DataAccess.Data.AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await PopulateDropdowns(ViewData);
            var products = await _context.Products
                .Include(p => p.ProductGroup)
                .Include(p => p.Supplier) // YENİ: Tedarikçi ilişkisi eklendi
                .Include(p => p.FscType)
                .Include(p => p.PaperType)
                .Include(p => p.PaperColor)
                .Include(p => p.PaperWeight)
                .Include(p => p.PaperWidth)
                .OrderBy(p => p.ProductCode)
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return Json(new { success = false, message = "KAYIT BULUNAMADI." });

            return Json(new
            {
                success = true,
                id = product.Id,
                productGroupId = product.ProductGroupId,
                supplierId = product.SupplierId, // YENİ: Tedarikçi ID eklendi
                productCode = product.ProductCode,
                productName = product.ProductName,
                unit = product.Unit,
                fscTypeId = product.FscTypeId,
                paperTypeId = product.PaperTypeId,
                paperColorId = product.PaperColorId,
                paperWeightId = product.PaperWeightId,
                paperWidthId = product.PaperWidthId,
                isActive = product.IsActive
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(Product model)
        {
            if (string.IsNullOrWhiteSpace(model.ProductName))
            {
                TempData["Error"] = "ÜRÜN ADI ZORUNLUDUR.";
                return RedirectToAction(nameof(Index));
            }

            if (model.Id == 0)
            {
                var group = await _context.ProductGroups.FindAsync(model.ProductGroupId);
                if (group == null)
                {
                    TempData["Error"] = "GEÇERLİ BİR ÜRÜN GRUBU SEÇİLMELİDİR.";
                    return RedirectToAction(nameof(Index));
                }

                // --- OTOMATİK KOD ÜRETME ---
                var lastProduct = await _context.Products
                    .Where(p => p.ProductGroupId == model.ProductGroupId)
                    .OrderByDescending(p => p.ProductCode)
                    .FirstOrDefaultAsync();

                string newCode;
                if (lastProduct == null)
                {
                    newCode = group.RangeStart.ToString();
                }
                else
                {
                    if (long.TryParse(lastProduct.ProductCode, out long lastCodeInt))
                    {
                        long nextCode = lastCodeInt + 1;
                        if (nextCode > group.RangeEnd)
                        {
                            TempData["Error"] = "BU GRUP İÇİN TANIMLANAN KOD ARALIĞI DOLMUŞTUR!";
                            return RedirectToAction(nameof(Index));
                        }
                        newCode = nextCode.ToString();
                    }
                    else
                    {
                        newCode = group.RangeStart.ToString();
                    }
                }

                model.ProductCode = newCode;
                model.ExternalCode = string.IsNullOrWhiteSpace(model.ExternalCode) ? null : model.ExternalCode.Trim().ToUpperInvariant();
                model.SupplierId = model.SupplierId; // Tedarikçi ataması
                model.Unit = model.Unit ?? "ADET";
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = "SYSTEM";
                model.IsActive = true;
                _context.Products.Add(model);
            }
            else
            {
                var existing = await _context.Products.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.ProductName = model.ProductName;
                    existing.ProductGroupId = model.ProductGroupId;
                    existing.SupplierId = model.SupplierId; // YENİ: Tedarikçi güncelleme
                    existing.Unit = model.Unit ?? "ADET";
                    existing.FscTypeId = model.FscTypeId;
                    existing.PaperTypeId = model.PaperTypeId;
                    existing.PaperColorId = model.PaperColorId;
                    existing.PaperWeightId = model.PaperWeightId;
                    existing.PaperWidthId = model.PaperWidthId;
                    existing.ExternalCode = string.IsNullOrWhiteSpace(model.ExternalCode) ? existing.ExternalCode : model.ExternalCode.Trim().ToUpperInvariant();
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = "SYSTEM";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return Json(new { success = false, message = "ÜRÜN BULUNAMADI." });

            product.IsActive = !product.IsActive;
            product.UpdatedDate = DateTime.Now;
            product.UpdatedBy = "SYSTEM";
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = product.IsActive });
        }

        private async Task PopulateDropdowns(ViewDataDictionary viewData)
        {
            viewData["ProductGroups"] = new SelectList(await _context.ProductGroups.Where(g => g.IsActive).OrderBy(g => g.GroupName).ToListAsync(), "Id", "GroupName");
            viewData["FscTypes"] = new SelectList(await _context.FscTypes.Where(f => f.IsActive).ToListAsync(), "Id", "Name");
            viewData["PaperTypes"] = new SelectList(await _context.PaperTypes.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
            viewData["PaperColors"] = new SelectList(await _context.PaperColors.Where(c => c.IsActive).ToListAsync(), "Id", "Name");

            // YENİ: Tedarikçi Listesi [cite: 2026-03-04]
            viewData["Suppliers"] = new SelectList(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");

            // PaperWeight ve PaperWidth listeleri
            var weights = await _context.PaperWeights.Where(w => w.IsActive).OrderBy(w => w.Value).ToListAsync();
            viewData["PaperWeights"] = new SelectList(weights.Select(x => new { Id = x.Id, Text = $"{x.Value} {x.Unit}" }), "Id", "Text");

            var widths = await _context.PaperWidths.Where(w => w.IsActive).OrderBy(w => w.Value).ToListAsync();
            viewData["PaperWidths"] = new SelectList(widths.Select(x => new { Id = x.Id, Text = $"{x.Value} {x.Unit}" }), "Id", "Text");
        }

        public async Task<IActionResult> ExportIndex()
        {
            var data = await _context.Products
                .Include(p => p.ProductGroup)
                .Include(p => p.Supplier) // Excel'e tedarikçi eklendi
                .Include(p => p.PaperWeight)
                .Include(p => p.PaperWidth)
                .OrderBy(p => p.ProductCode)
                .Select(p => new {
                    p.ProductCode,
                    p.ProductName,
                    Tedarikci = p.Supplier.Name ?? "-",
                    Grup = p.ProductGroup.GroupName,
                    p.Unit,
                    Gramaj = p.PaperWeight != null ? $"{p.PaperWeight.Value} {p.PaperWeight.Unit}" : "-",
                    En = p.PaperWidth != null ? $"{p.PaperWidth.Value} {p.PaperWidth.Unit}" : "-",
                    Durum = p.IsActive ? "AKTİF" : "PASİF"
                }).ToListAsync();

            return ExportToExcel(data, "UrunListesi");
        }

        // ─── Ürün Reçetesi (BOM) ─────────────────────────────────────────────

        // GET /Products/Recipe/{id}
        public async Task<IActionResult> Recipe(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductGroup)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var lines = await _context.ProductRecipes
                .Include(r => r.ChildProduct).ThenInclude(c => c.ProductGroup)
                .Where(r => r.ParentProductId == id)
                .OrderBy(r => r.ChildProduct.ProductName)
                .ToListAsync();

            // Bileşen olarak eklenebilecek ürünler (kendisi hariç)
            ViewBag.AvailableProducts = await _context.Products
                .Where(p => p.IsActive && p.Id != id)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewData["Title"] = $"Reçete — {product.ProductName}";
            ViewBag.Product = product;
            return View(lines);
        }

        // POST /Products/SaveRecipeLine
        [HttpPost]
        public async Task<IActionResult> SaveRecipeLine(ProductRecipe model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var exists = await _context.ProductRecipes.AnyAsync(r =>
                        r.ParentProductId == model.ParentProductId &&
                        r.ChildProductId  == model.ChildProductId);
                    if (exists)
                        return Json(new { success = false, message = "Bu bileşen zaten reçetede mevcut." });

                    model.IsActive    = true;
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy   = User.Identity?.Name ?? "System";
                    _context.ProductRecipes.Add(model);
                }
                else
                {
                    var existing = await _context.ProductRecipes.FindAsync(model.Id);
                    if (existing == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                    existing.ChildProductId    = model.ChildProductId;
                    existing.StandardQuantity  = model.StandardQuantity;
                    existing.Unit              = model.Unit;
                    existing.UpdatedDate       = DateTime.Now;
                    existing.UpdatedBy         = User.Identity?.Name ?? "System";
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Reçete satırı kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Products/GetRecipeLine
        [HttpGet]
        public async Task<IActionResult> GetRecipeLine(int id)
        {
            var item = await _context.ProductRecipes.FindAsync(id);
            if (item == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                item.Id, item.ParentProductId, item.ChildProductId,
                item.StandardQuantity, item.Unit, item.IsActive
            }});
        }

        // POST /Products/DeleteRecipeLine
        [HttpPost]
        public async Task<IActionResult> DeleteRecipeLine(int id)
        {
            var item = await _context.ProductRecipes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.ProductRecipes.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST /Products/ToggleRecipeLine
        [HttpPost]
        public async Task<IActionResult> ToggleRecipeLine(int id)
        {
            var item = await _context.ProductRecipes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            item.IsActive    = !item.IsActive;
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy   = User.Identity?.Name ?? "System";
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive });
        }

        // GET /Products/GetRecipeForWorkOrder/{productId}  — üretim sayfasından çağrılır
        [HttpGet]
        public async Task<IActionResult> GetRecipeForWorkOrder(int productId)
        {
            var lines = await _context.ProductRecipes
                .Include(r => r.ChildProduct)
                .Where(r => r.ParentProductId == productId && r.IsActive)
                .OrderBy(r => r.ChildProduct.ProductName)
                .Select(r => new {
                    r.Id,
                    r.ChildProductId,
                    childProductName = r.ChildProduct.ProductName,
                    childProductCode = r.ChildProduct.ProductCode,
                    r.StandardQuantity,
                    r.Unit
                })
                .ToListAsync();

            return Json(new { success = true, lines });
        }
    }
}