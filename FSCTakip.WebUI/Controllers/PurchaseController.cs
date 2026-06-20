using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class PurchaseController : BaseController
    {
        private readonly IFileStorageService _storage;

        public PurchaseController(AppDbContext context, IFileStorageService storage) : base(context)
        {
            _storage = storage;
        }

        // GET /Purchase/Index
        public async Task<IActionResult> Index(int[]? supplierIds, int[]? fscTypeIds, string? stockCode, string? stockName, int[]? productIds)
        {
            var query = _context.FscLots
                .Include(l => l.Supplier)
                .Include(l => l.FscType)
                .Include(l => l.Product).ThenInclude(p => p!.PaperWeight)
                .Include(l => l.Product).ThenInclude(p => p!.PaperWidth)
                .Include(l => l.Product).ThenInclude(p => p!.PaperColor)
                .Include(l => l.Product).ThenInclude(p => p!.ProductGroup)
                .Include(l => l.Serials)
                // Yalnizca dogrudan satin alma girisleri -- donusum ciktilari (SourceSerialId dolu) haric
                .Where(l => l.SourceSerialId == null)
                .AsQueryable();

            if (supplierIds != null && supplierIds.Length > 0)
                query = query.Where(l => l.SupplierId.HasValue && supplierIds.Contains(l.SupplierId.Value));
            if (fscTypeIds != null && fscTypeIds.Length > 0)
                query = query.Where(l => fscTypeIds.Contains(l.FscTypeId));
            if (!string.IsNullOrWhiteSpace(stockCode))
            {
                var sc = stockCode.Trim();
                query = query.Where(l => l.Product != null &&
                    (l.Product.ProductCode.Contains(sc) || (l.Product.ExternalCode != null && l.Product.ExternalCode.Contains(sc))));
            }
            if (!string.IsNullOrWhiteSpace(stockName))
                query = query.Where(l => l.Product != null && l.Product.ProductName.Contains(stockName.Trim()));
            if (productIds != null && productIds.Length > 0)
                query = query.Where(l => l.ProductId.HasValue && productIds.Contains(l.ProductId.Value));

            ViewBag.Suppliers   = await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.FscTypes    = await _context.FscTypes.Where(f => f.IsActive).ToListAsync();

            var products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.Products    = products;
            ViewBag.StockCode   = stockCode;
            ViewBag.StockName   = stockName;
            ViewBag.SupplierIds = supplierIds ?? Array.Empty<int>();
            ViewBag.FscTypeIds  = fscTypeIds  ?? Array.Empty<int>();
            ViewBag.ProductIds  = productIds  ?? Array.Empty<int>();

            // Urun basina birim ve donusum katsayisi -- iki ayri sozluk (value tuple ViewBag'den cast edilmez)
            var conversions    = await _context.UnitConversions.Where(c => c.IsActive).ToListAsync();
            var productUnits   = new Dictionary<int, string>();
            var productFactors = new Dictionary<int, decimal>();
            foreach (var p in products)
            {
                var unit   = (p.Unit ?? "KG").Trim().ToUpperInvariant();
                var factor = UnitConversionController.FindFactor(conversions, unit, p.Id, p.ProductGroupId)
                             ?? (unit == "KG" ? 1m : 0m);
                productUnits[p.Id]   = unit;
                productFactors[p.Id] = factor;
            }
            ViewBag.ProductUnits   = productUnits;
            ViewBag.ProductFactors = productFactors;

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
        // serialsJson: JSON dizi -- yeni giris modalindan bobin agırlıkları gelir
        // Ornek: "[500.5,480.0,520.0]"  veya esit mod icin "equal:10:500" (count:weight)
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
                        return Json(new { success = false, message = "Tedarikci bulunamadi." });
                }

                // Tedarikci zorunlu (FSC hammadde takibi icin)
                if (!model.SupplierId.HasValue || model.SupplierId.Value == 0)
                    return Json(new { success = false, message = "Tedarikci secimi zorunludur. FSC hammadde girisinde sertifikali tedarikci belirtilmelidir." });

                if (!model.ProductId.HasValue || model.ProductId.Value == 0)
                    return Json(new { success = false, message = "Urun secimi zorunludur." });

                // Parti no zorunlu
                if (string.IsNullOrWhiteSpace(model.PartiNo))
                    return Json(new { success = false, message = "Parti numarasi zorunludur." });

                if (invoiceFile != null && invoiceFile.Length > 0)
                    model.InvoicePdfPath = await _storage.SaveAsync(invoiceFile, "Invoice");

                if (dispatchFile != null && dispatchFile.Length > 0)
                    model.DispatchPdfPath = await _storage.SaveAsync(dispatchFile, "Dispatch");

                string fscUyari = string.Empty;
                if (supplier != null && supplier.IsFscActive && supplier.FscExpiryDate.HasValue && supplier.FscExpiryDate.Value < DateTime.Today)
                    fscUyari = $"{supplier.Name} firmasinin FSC sertifikasi gecersiz veya suresi dolmus!";

                if (model.Id == 0)
                    _context.FscLots.Add(model);
                else
                    _context.FscLots.Update(model);

                await _context.SaveChangesAsync();

                // -- Bobinleri kaydet (yalnizca yeni lot icin) --------
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
                        catch { /* gecersiz JSON -- bobin eklenmez */ }
                    }

                    // Urun birimini KG'ye cevir -- server tarafinda UnitConversion tablosuyla
                    var product = model.ProductId.HasValue
                        ? await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.ProductId.Value)
                        : null;
                    var productUnit = product?.Unit?.Trim().ToUpperInvariant() ?? "KG";
                    decimal? convFactor = null;
                    if (productUnit != "KG")
                    {
                        var unitConvs = await _context.UnitConversions.Where(c => c.IsActive).ToListAsync();
                        convFactor = UnitConversionController.FindFactor(unitConvs, productUnit, product?.Id, product?.ProductGroupId);
                    }

                    for (int i = 0; i < weights.Count; i++)
                    {
                        if (weights[i] <= 0) continue;
                        var rawQty = weights[i];
                        var kgQty  = convFactor.HasValue ? rawQty * convFactor.Value : rawQty;
                        var serial = new FscSerial
                        {
                            LotId            = model.Id,
                            SerialNo         = $"{model.PartiNo}-B{i + 1:D2}",
                            InitialWeight    = kgQty,
                            CurrentWeight    = kgQty,
                            OriginalQuantity = convFactor.HasValue ? rawQty : null,
                            OriginalUnit     = convFactor.HasValue ? productUnit : null,
                            CreatedDate      = DateTime.Now,
                            CreatedBy        = User.Identity?.Name ?? "System"
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

                            // StockMovement: orijinal birim ve miktar ile kaydet
                            if (model.ProductId.HasValue)
                            {
                                var rawTotal = weights.Where(w => w > 0).Sum();
                                _context.StockMovements.Add(new StockMovement
                                {
                                    Type             = MovementType.PurchaseEntry,
                                    ProductId        = model.ProductId.Value,
                                    Quantity         = rawTotal,
                                    Unit             = productUnit,
                                    QuantityKg       = convFactor.HasValue ? rawTotal * convFactor.Value : rawTotal,
                                    DocumentNo       = model.DispatchNo ?? model.PartiNo,
                                    DocumentDate     = model.ArrivalDate,
                                    Description      = $"Hammadde girisi -- {model.PartiNo} ({serialCount} bobin)",
                                    CreatedDate      = DateTime.Now,
                                    CreatedBy        = User.Identity?.Name ?? "System"
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
                    message     = $"Hammadde girisi kaydedildi.",
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

            ViewData["Title"] = $"Lot Detayi -- {lot.PartiNo}";
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
                    return Json(new { success = false, message = "Bobin agırlıgı sıfırdan buyuk olmalıdır." });

                var lot = await _context.FscLots
                    .Include(l => l.Product)
                    .FirstOrDefaultAsync(l => l.Id == model.LotId);
                if (lot == null)
                    return Json(new { success = false, message = "Lot bulunamadı." });

                // -- Birim donusumu: urun birimi KG degilse otomatik uygula --
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
                    // Seri numarası otomatik uret
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
                    // CurrentWeight: sadece hic tuketim yoksa guncelle
                    if (!existing.ProductionDetails.Any())
                        existing.CurrentWeight = existing.InitialWeight;
                }

                await _context.SaveChangesAsync();

                if (lot.ProductId.HasValue)
                {
                    string docNo   = lot.DispatchNo ?? lot.PartiNo;
                    string smDescr = $"Bobin giris -- {model.SerialNo} ({lot.PartiNo})";

                    // Yeni seri ekleme: SM yoksa ekle; varsa lot toplamını güncelle
                    // Her lot için tek bir SM kaydı tutulur (lot bazlı toplam)
                    var lotSm = await _context.StockMovements
                        .Where(m => m.Type == MovementType.PurchaseEntry
                                 && m.ProductId == lot.ProductId.Value
                                 && m.DocumentNo == docNo)
                        .FirstOrDefaultAsync();

                    // SaveChanges'tan sonra tüm seri ağırlıklarını yeniden topla (güncel değerlerle)
                    var allSerialWeights = await _context.FscSerials
                        .Where(s => s.LotId == lot.Id)
                        .Select(s => new { s.InitialWeight, s.OriginalQuantity, s.OriginalUnit })
                        .ToListAsync();

                    decimal totalOriginal = allSerialWeights
                        .Sum(s => s.OriginalQuantity ?? s.InitialWeight);
                    decimal totalKg = allSerialWeights.Sum(s => s.InitialWeight);
                    bool hasOriginalUnit = allSerialWeights.Any(s => s.OriginalQuantity.HasValue);

                    if (lotSm == null)
                    {
                        // İlk seri: SM oluştur
                        _context.StockMovements.Add(new StockMovement
                        {
                            Type         = MovementType.PurchaseEntry,
                            ProductId    = lot.ProductId.Value,
                            Quantity     = hasOriginalUnit ? totalOriginal : totalKg,
                            Unit         = hasOriginalUnit ? productUnit : "KG",
                            QuantityKg   = hasOriginalUnit ? totalKg : null,
                            DocumentNo   = docNo,
                            DocumentDate = lot.ArrivalDate,
                            Description  = smDescr,
                            CreatedDate  = DateTime.Now,
                            CreatedBy    = User.Identity?.Name ?? "System"
                        });
                    }
                    else
                    {
                        // Seri eklendi veya düzenlendi: SM lotun güncel toplamıyla eşitle
                        lotSm.Quantity    = hasOriginalUnit ? totalOriginal : totalKg;
                        lotSm.Unit        = hasOriginalUnit ? productUnit : "KG";
                        lotSm.QuantityKg  = hasOriginalUnit ? totalKg : null;
                        lotSm.UpdatedDate = DateTime.Now;
                        lotSm.UpdatedBy   = User.Identity?.Name ?? "System";
                    }
                    await _context.SaveChangesAsync();
                }

                var unitMsg = convFactor.HasValue
                    ? $"Seri kaydedildi. {enteredQty:N3} {productUnit} -> {weightKg:N4} KG olarak donusturuldu."
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
                    return Json(new { success = false, message = "Bu seriye baglı uretim kaydı var, silinemez." });

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
                    return Json(new { success = false, message = "Dosya secilmedi." });

                var lot = await _context.FscLots.FindAsync(lotId);
                if (lot == null) return Json(new { success = false, message = "Lot bulunamadı." });

                var path = await _storage.SaveAsync(file, docType == "invoice" ? "Invoice" : "Dispatch");

                if (docType == "invoice")
                    lot.InvoicePdfPath = path;
                else
                    lot.DispatchPdfPath = path;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Belge yuklendi.", path });
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