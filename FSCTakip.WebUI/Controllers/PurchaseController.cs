using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class PurchaseController : BaseController
    {
        private readonly IWebHostEnvironment _env;

        public PurchaseController(AppDbContext context, IWebHostEnvironment env) : base(context)
        {
            _env = env;
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
                .Include(l => l.Serials)
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

                // Parti no zorunlu
                if (string.IsNullOrWhiteSpace(model.PartiNo))
                    return Json(new { success = false, message = "Parti numarası zorunludur." });

                if (invoiceFile != null && invoiceFile.Length > 0)
                    model.InvoicePdfPath = await SaveFile(invoiceFile, "purchases");

                if (dispatchFile != null && dispatchFile.Length > 0)
                    model.DispatchPdfPath = await SaveFile(dispatchFile, "purchases");

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
                var lot = await _context.FscLots.FindAsync(model.LotId);
                if (lot == null)
                    return Json(new { success = false, message = "Lot bulunamadı." });

                if (model.Id == 0)
                {
                    // Seri numarası otomatik üret
                    if (string.IsNullOrWhiteSpace(model.SerialNo))
                    {
                        var count = await _context.FscSerials.CountAsync(s => s.LotId == model.LotId);
                        model.SerialNo = $"{lot.PartiNo}-B{count + 1:D2}";
                    }
                    model.CurrentWeight = model.InitialWeight;
                    _context.FscSerials.Add(model);
                }
                else
                {
                    var existing = await _context.FscSerials.FindAsync(model.Id);
                    if (existing == null) return Json(new { success = false, message = "Seri bulunamadı." });

                    existing.SerialNo      = model.SerialNo;
                    existing.LotNo         = model.LotNo;
                    existing.InitialWeight = model.InitialWeight;
                    existing.IsOpeningStock = model.IsOpeningStock;
                    existing.Notes         = model.Notes;
                    // CurrentWeight sadece giriş ağırlığı değiştiyse ve hiç tüketim yoksa güncelle
                    if (!existing.ProductionDetails.Any())
                        existing.CurrentWeight = model.InitialWeight;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Seri kaydedildi." });
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

                var path = await SaveFile(file, "purchases");

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

        // GET /Purchase/ViewDocument
        public IActionResult ViewDocument(string path)
        {
            var full = Path.Combine(_env.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(full)) return NotFound("Belge bulunamadı.");
            return PhysicalFile(full, "application/pdf");
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

        private async Task<string> SaveFile(IFormFile file, string subfolder)
        {
            var yearMonth = DateTime.Now.ToString("yyyy/MM");
            var dir = Path.Combine(_env.WebRootPath, "uploads", subfolder, yearMonth);
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var full = Path.Combine(dir, name);
            using var stream = new FileStream(full, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subfolder}/{yearMonth}/{name}";
        }
    }
}
