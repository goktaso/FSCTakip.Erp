using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    [Authorize]
    public class PurchaseController : BaseController
    {
        private readonly IFileStorageService _storage;

        public PurchaseController(AppDbContext context, IFileStorageService storage) : base(context)
        {
            _storage = storage;
        }

        // GET /Purchase/Index
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? supplierId, int? fscTypeId)
        {
            var query = _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product).ThenInclude(p => p!.PaperWeight)
                .Include(l => l.Product).ThenInclude(p => p!.PaperWidth)
                .Include(l => l.Product).ThenInclude(p => p!.PaperColor)
                .Include(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Include(l => l.Serials)
                // Bu ekran HAMMADDE girişi/alımı içindir. Dışarıda tutulanlar:
                //  • Dönüşümle içeride üretilen yarı mamüller (SourceSerialId dolu)
                //  • Hammadde olmayan ürünler (yarı mamül 2xxx / mamul 3xxx / sarf 4xxx) — bunlar
                //    dönüşüm/üretim çıktısıdır, satın alma değil.
                // Yarı mamül/mamul stoğu Hammadde Stoğu ve Yarı Mamül Dönüşüm ekranlarında görünür.
                .Where(l => l.SourceSerialId == null
                         && (l.Product == null
                             || l.Product.ExternalCode == null
                             || l.Product.ExternalCode.StartsWith("1")
                             || (l.Product.ProductGroup != null && l.Product.ProductGroup.GroupName.Contains("HAMMADDE"))))
                .AsQueryable();

            if (startDate.HasValue)  query = query.Where(l => l.ArrivalDate >= startDate.Value);
            if (endDate.HasValue)    query = query.Where(l => l.ArrivalDate <= endDate.Value.AddDays(1));
            if (supplierId.HasValue) query = query.Where(l => l.SupplierId == supplierId.Value);
            if (fscTypeId.HasValue)  query = query.Where(l => l.FscTypeId == fscTypeId.Value);

            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.FscTypes  = await _context.FscTypes.Where(f => f.IsActive).ToListAsync();
            ViewBag.Products  = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate   = endDate?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(l => l.Id).ToListAsync());
        }

        // GET /Purchase/GetLot/{id}
        [HttpGet]
        public async Task<IActionResult> GetLot(int id)
        {
            var lot = await _context.FscLots.FindAsync(id);
            if (lot == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                lot.Id, lot.PartiNo, lot.SupplierId, lot.FscTypeId, lot.ProductId,
                lot.InvoiceNo, lot.DispatchNo,
                arrivalDate = lot.ArrivalDate.ToString("yyyy-MM-dd"),
                lot.TruckPlate, lot.InvoiceAmount, lot.Currency, lot.Notes
            }});
        }

        // POST /Purchase/SaveLot
        // serialsJson: JSON dizi — yeni giriş modalından bobin ağırlıkları gelir
        // Örnek: "[500.5,480.0,520.0]"  veya eşit mod için "equal:10:500" (count:weight)
        [HttpPost]
        public async Task<IActionResult> SaveLot(
            FscLot model,
            IFormFile? invoiceFile,
            IFormFile? dispatchFile,
            string? serialsJson)
        {
            try
            {
                Supplier? supplier = null;
                if (model.SupplierId.HasValue)
                {
                    supplier = await _context.Suppliers.FindAsync(model.SupplierId.Value);
                    if (supplier == null)
                        return Json(new { success = false, message = "Tedarikçi bulunamadı." });
                }

                // Tedarikçi zorunlu (FSC hammadde takibi için)
                if (!model.SupplierId.HasValue || model.SupplierId.Value == 0)
                    return Json(new { success = false, message = "Tedarikçi seçimi zorunludur. FSC hammadde girişinde sertifikalı tedarikçi belirtilmelidir." });

                if (!model.ProductId.HasValue || model.ProductId.Value == 0)
                    return Json(new { success = false, message = "Ürün seçimi zorunludur." });

                // Parti no zorunlu
                if (string.IsNullOrWhiteSpace(model.PartiNo))
                    return Json(new { success = false, message = "Parti numarası zorunludur." });

                if (invoiceFile != null && invoiceFile.Length > 0)
                    model.InvoicePdfPath = await _storage.SaveAsync(invoiceFile, "Invoice");

                if (dispatchFile != null && dispatchFile.Length > 0)
                    model.DispatchPdfPath = await _storage.SaveAsync(dispatchFile, "Dispatch");

                string fscUyari = string.Empty;
                if (supplier != null && (!supplier.IsFscActive || (supplier.FscExpiryDate.HasValue && supplier.FscExpiryDate.Value < DateTime.Today)))
                    fscUyari = $"{supplier.Name} firmasının FSC sertifikası geçersiz veya süresi dolmuş!";

                if (model.Id == 0)
                    _context.FscLots.Add(model);
                else
                    _context.FscLots.Update(model);

                await _context.SaveChangesAsync();

                // ── Bobinleri kaydet (yalnızca yeni lot için) ──────────────
                int serialCount = 0;
                if (model.Id > 0 && !string.IsNullOrWhiteSpace(serialsJson))
                {
                    var weights = new List<decimal>();

                    if (serialsJson.StartsWith("equal:"))
                    {
                        // Format: "equal:COUNT:WEIGHT"
                        var parts = serialsJson.Split(':');
                        if (parts.Length == 3
                            && int.TryParse(parts[1], out int cnt)
                            && decimal.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                                                System.Globalization.CultureInfo.InvariantCulture, out decimal wgt)
                            && cnt > 0 && wgt > 0)
                        {
                            for (int i = 0; i < cnt; i++) weights.Add(wgt);
                        }
                    }
                    else
                    {
                        // Format: JSON dizi "[500.5,480.0]"
                        try
                        {
                            weights = System.Text.Json.JsonSerializer.Deserialize<List<decimal>>(serialsJson)
                                      ?? new List<decimal>();
                        }
                        catch { /* geçersiz JSON — bobin eklenmez */ }
                    }

                    for (int i = 0; i < weights.Count; i++)
                    {
                        if (weights[i] <= 0) continue;
                        var serial = new FscSerial
                        {
                            LotId         = model.Id,
                            SerialNo      = $"{model.PartiNo}-B{i + 1:D2}",
                            InitialWeight = weights[i],
                            CurrentWeight = weights[i],
                            CreatedDate   = DateTime.Now,
                            CreatedBy     = User.Identity?.Name ?? "System"
                        };
                        _context.FscSerials.Add(serial);
                        serialCount++;
                    }

                    if (serialCount > 0)
                    {
                        using var tx = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            await _context.SaveChangesAsync();

                            // StockMovement: hammadde giriş kaydı oluştur
                            if (model.ProductId.HasValue)
                            {
                                var totalKg = weights.Where(w => w > 0).Sum();
                                _context.StockMovements.Add(new StockMovement
                                {
                                    Type         = MovementType.PurchaseEntry,
                                    ProductId    = model.ProductId.Value,
                                    Quantity     = totalKg,
                                    Unit         = "kg",
                                    DocumentNo   = model.DispatchNo ?? model.PartiNo,
                                    DocumentDate = model.ArrivalDate,
                                    Description  = $"Hammadde girişi — {model.PartiNo} ({serialCount} bobin)",
                                    CreatedDate  = DateTime.Now,
                                    CreatedBy    = User.Identity?.Name ?? "System"
                                });
                                await _context.SaveChangesAsync();
                            }
                            await tx.CommitAsync();
                        }
                        catch
                        {
                            await tx.RollbackAsync();
                            throw;
                        }
                    }
                }

                return Json(new {
                    success     = true,
                    message     = $"Hammadde girişi kaydedildi.",
                    lotId       = model.Id,
                    partiNo     = model.PartiNo,
                    serialCount,
                    fscUyari
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Purchase/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var lot = await _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product)
                .Include(l => l.Serials)
                    .ThenInclude(s => s.ProductionDetails)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lot == null) return NotFound();

            ViewData["Title"] = $"Lot Detayı — {lot.PartiNo}";
            return View(lot);
        }

        // GET /Purchase/GetSerial/{id}
        [HttpGet]
        public async Task<IActionResult> GetSerial(int id)
        {
            var s = await _context.FscSerials.FindAsync(id);
            if (s == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                s.Id, s.SerialNo, s.LotNo, s.InitialWeight, s.CurrentWeight, s.IsOpeningStock, s.Notes
            }});
        }

        // POST /Purchase/SaveSerial
        [HttpPost]
        public async Task<IActionResult> SaveSerial(FscSerial model)
        {
            try
            {
                if (model.InitialWeight <= 0)
                    return Json(new { success = false, message = "Bobin ağırlığı sıfırdan büyük olmalıdır." });

                var lot = await _context.FscLots
                    .Include(l => l.Product)
                    .FirstOrDefaultAsync(l => l.Id == model.LotId);
                if (lot == null)
                    return Json(new { success = false, message = "Lot bulunamadı." });

                // ── Birim dönüşümü: ürün birimi KG değilse otomatik uygula ──
                var productUnit = lot.Product?.Unit?.Trim().ToUpperInvariant() ?? "KG";
                decimal? convFactor = null;
                if (productUnit != "KG" && productUnit != "")
                {
                    var conversions = await _context.UnitConversions.Where(c => c.IsActive).ToListAsync();
                    convFactor = UnitConversionController.FindFactor(
                        conversions, productUnit, lot.ProductId, lot.Product?.ProductGroupId);
                }

                decimal enteredQty = model.InitialWeight;
                decimal weightKg   = convFactor.HasValue ? enteredQty * convFactor.Value : enteredQty;

                if (model.Id == 0)
                {
                    // Seri numarası otomatik üret
                    if (string.IsNullOrWhiteSpace(model.SerialNo))
                    {
                        var count = await _context.FscSerials.CountAsync(s => s.LotId == model.LotId);
                        model.SerialNo = $"{lot.PartiNo}-B{count + 1:D2}";
                    }
                    if (convFactor.HasValue)
                    {
                        model.OriginalQuantity = enteredQty;
                        model.OriginalUnit     = productUnit;
                        model.InitialWeight    = weightKg;
                    }
                    model.CurrentWeight = model.InitialWeight;
                    _context.FscSerials.Add(model);
                }
                else
                {
                    var existing = await _context.FscSerials
                        .Include(s => s.ProductionDetails)
                        .FirstOrDefaultAsync(s => s.Id == model.Id);
                    if (existing == null) return Json(new { success = false, message = "Seri bulunamadı." });

                    existing.SerialNo       = model.SerialNo;
                    existing.LotNo          = model.LotNo;
                    existing.IsOpeningStock = model.IsOpeningStock;
                    existing.Notes          = model.Notes;

                    if (convFactor.HasValue)
                    {
                        existing.OriginalQuantity = enteredQty;
                        existing.OriginalUnit     = productUnit;
                        existing.InitialWeight    = weightKg;
                    }
                    else
                    {
                        existing.InitialWeight = model.InitialWeight;
                    }
                    // CurrentWeight: sadece hiç tüketim yoksa güncelle
                    if (!existing.ProductionDetails.Any())
                        existing.CurrentWeight = existing.InitialWeight;
                }

                await _context.SaveChangesAsync();

                var unitMsg = convFactor.HasValue
                    ? $"Seri kaydedildi. {enteredQty:N3} {productUnit} → {weightKg:N4} KG olarak dönüştürüldü."
                    : "Seri kaydedildi.";
                return Json(new { success = true, message = unitMsg, convertedKg = weightKg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Purchase/DeleteSerial
        [HttpPost]
        public async Task<IActionResult> DeleteSerial(int id)
        {
            try
            {
                var serial = await _context.FscSerials
                    .Include(s => s.ProductionDetails)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (serial == null) return Json(new { success = false, message = "Seri bulunamadı." });
                if (serial.ProductionDetails.Any())
                    return Json(new { success = false, message = "Bu seriye bağlı üretim kaydı var, silinemez." });

                _context.FscSerials.Remove(serial);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Seri silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Purchase/UploadDocument
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int lotId, string docType, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Dosya seçilmedi." });

                var lot = await _context.FscLots.FindAsync(lotId);
                if (lot == null) return Json(new { success = false, message = "Lot bulunamadı." });

                var path = await _storage.SaveAsync(file, docType == "invoice" ? "Invoice" : "Dispatch");

                if (docType == "invoice")
                    lot.InvoicePdfPath = path;
                else
                    lot.DispatchPdfPath = path;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Belge yüklendi.", path });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Purchase/ExportIndex
        public async Task<IActionResult> ExportIndex()
        {
            var lots = await _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product).ThenInclude(p => p!.PaperWeight)
                .Include(l => l.Product).ThenInclude(p => p!.PaperWidth)
                .Include(l => l.Product).ThenInclude(p => p!.PaperColor)
                .Include(l => l.Serials)
                .OrderByDescending(l => l.Id).ToListAsync();

            var rows = lots.Select(l => new {
                PartiNo      = l.PartiNo,
                Tedarikci    = l.Supplier?.Name,
                FscTipi      = l.FscType?.Name,
                ErpStokKodu  = l.Product?.ExternalCode ?? "",
                StokKodu     = l.Product?.ProductCode ?? "",
                StokAdi      = l.Product?.ProductName ?? "",
                Gramaj       = l.Product?.PaperWeight != null ? $"{l.Product.PaperWeight.Value} {l.Product.PaperWeight.Unit}" : "",
                KagitEni     = l.Product?.PaperWidth  != null ? $"{l.Product.PaperWidth.Value} mm" : "",
                Renk         = l.Product?.PaperColor?.Name ?? "",
                GelisTarihi  = l.ArrivalDate.ToString("dd.MM.yyyy"),
                IrsaliyeNo   = l.DispatchNo,
                FaturaNo     = l.InvoiceNo,
                BobinAdedi   = l.Serials.Count,
                ToplamKg     = l.Serials.Sum(s => s.InitialWeight),
                KalanKg      = l.Serials.Sum(s => s.CurrentWeight)
            });

            return ExportToExcel(rows, "HammaddeGirisleri");
        }
    }
}
