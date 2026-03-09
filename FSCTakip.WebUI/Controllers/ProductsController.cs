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

// ... Mevcut using satırları ...

namespace FSC_ERP.Controllers
{
    public class ProductsController : BaseController
    {
        public ProductsController(FSCTakip.DataAccess.Data.AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            await PopulateDropdowns(ViewData);
            var products = await _context.Products
                .Include(p => p.ProductGroup)
                .Include(p => p.Supplier)
                .Include(p => p.FscType)
                .Include(p => p.PaperType)
                .Include(p => p.PaperColor)
                .Include(p => p.PaperWeight)
                .Include(p => p.PaperWidth)
                .Include(p => p.Unit) // YENİ: Birim ilişkisi eklendi
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
                supplierId = product.SupplierId,
                productCode = product.ProductCode,
                productName = product.ProductName,
                unitId = product.UnitId, // GÜNCELLENDİ: Artık UnitId dönüyoruz
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

                // --- OTOMATİK KOD ÜRETME --- (Mevcut mantık korunmuştur)
                var lastProduct = await _context.Products
                    .Where(p => p.ProductGroupId == model.ProductGroupId)
                    .OrderByDescending(p => p.ProductCode)
                    .FirstOrDefaultAsync();

                string newCode;
                if (lastProduct == null) { newCode = group.RangeStart.ToString(); }
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
                    else { newCode = group.RangeStart.ToString(); }
                }

                model.ProductCode = newCode;
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
                    existing.SupplierId = model.SupplierId;
                    existing.UnitId = model.UnitId; // GÜNCELLENDİ: UnitId ataması
                    existing.FscTypeId = model.FscTypeId;
                    existing.PaperTypeId = model.PaperTypeId;
                    existing.PaperColorId = model.PaperColorId;
                    existing.PaperWeightId = model.PaperWeightId;
                    existing.PaperWidthId = model.PaperWidthId;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = "SYSTEM";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(ViewDataDictionary viewData)
        {
            viewData["ProductGroups"] = new SelectList(await _context.ProductGroups.Where(g => g.IsActive).OrderBy(g => g.GroupName).ToListAsync(), "Id", "GroupName");
            viewData["FscTypes"] = new SelectList(await _context.FscTypes.Where(f => f.IsActive).ToListAsync(), "Id", "Name");
            viewData["PaperTypes"] = new SelectList(await _context.PaperTypes.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
            viewData["PaperColors"] = new SelectList(await _context.PaperColors.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
            viewData["Suppliers"] = new SelectList(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");

            // YENİ: Birim Listesi Dropdown için eklendi
            viewData["Units"] = new SelectList(await _context.Units.Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync(), "Id", "Name");

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
                .Include(p => p.Supplier)
                .Include(p => p.Unit) // Excel için birim adı
                .Include(p => p.PaperWeight)
                .Include(p => p.PaperWidth)
                .OrderBy(p => p.ProductCode)
                .Select(p => new {
                    p.ProductCode,
                    p.ProductName,
                    Tedarikci = p.Supplier.Name ?? "-",
                    Grup = p.ProductGroup.GroupName,
                    Birim = p.Unit.Name ?? "-", // GÜNCELLENDİ
                    Gramaj = p.PaperWeight != null ? $"{p.PaperWeight.Value} {p.PaperWeight.Unit}" : "-",
                    En = p.PaperWidth != null ? $"{p.PaperWidth.Value} {p.PaperWidth.Unit}" : "-",
                    Durum = p.IsActive ? "AKTİF" : "PASİF"
                }).ToListAsync();

            return ExportToExcel(data, "UrunListesi");
        }
    }
}