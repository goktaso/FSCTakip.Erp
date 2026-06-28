using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    /// <summary>
    /// Yarı Mamül Dönüşüm — ham kağıt (1xxxx) / YM (24xxx) bobinini tüketip
    /// yarı mamül (23xxx BB) bobinine dönüştürür. Çıktı tekrar stok olur ve
    /// 3xxxx mamul üretiminde tüketilebilir. FSC tipi kaynaktan devralınır (CoC).
    /// </summary>
    public class ConversionController : BaseController
    {
        public ConversionController(AppDbContext context) : base(context) { }

        // GET /Conversion
        public async Task<IActionResult> Index()
        {
            // Stokta kalanı olan kaynak bobinler (ham/YM)
            ViewBag.SourceSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(s => s.CurrentWeight > 0)
                .OrderByDescending(s => s.Id)
                .Select(s => new ConvSourceVM {
                    Id            = s.Id,
                    SerialNo      = s.SerialNo,
                    CurrentWeight = s.CurrentWeight,
                    FscType       = s.Lot.FscType.Name,
                    UrunKod       = s.Lot.Product != null ? s.Lot.Product.ExternalCode : null,
                    UrunAd        = s.Lot.Product != null ? s.Lot.Product.ProductName : "—",
                    UrunIcKod     = s.Lot.Product != null ? s.Lot.Product.ProductCode : null,
                    PartiNo       = s.Lot.PartiNo
                })
                .ToListAsync();

            // Hedef yarı mamüller (2 ile başlayan kodlar)
            ViewBag.TargetProducts = await _context.Products
                .Where(p => p.ExternalCode != null && p.ExternalCode.StartsWith("2"))
                .OrderBy(p => p.ProductName)
                .Select(p => new ConvTargetVM { Id = p.Id, Kod = p.ExternalCode!, Ad = p.ProductName, ExtKod = p.ExternalCode })
                .ToListAsync();

            // Son dönüşümler (kaynak bobin/ürün bilgisiyle)
            var recentYm = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(s => s.Lot.PartiNo.StartsWith("YM"))
                .OrderByDescending(s => s.Id)
                .Take(50)
                .Select(s => new {
                    s.Id, LotId = s.LotId,
                    s.Lot.ArrivalDate, s.Lot.PartiNo,
                    Hedef   = s.Lot.Product != null ? s.Lot.Product.ProductName : "—",
                    FscType = s.Lot.FscType.Name,
                    s.InitialWeight, s.CurrentWeight,
                    s.Lot.SourceSerialId, s.Lot.ConversionFireKg
                })
                .ToListAsync();

            var srcIds = recentYm.Where(x => x.SourceSerialId != null).Select(x => x.SourceSerialId!.Value).Distinct().ToList();
            var srcMap = await _context.FscSerials
                .Where(s => srcIds.Contains(s.Id))
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Select(s => new {
                    s.Id, s.SerialNo,
                    Kod = s.Lot.Product != null ? s.Lot.Product.ExternalCode : null,
                    Ad  = s.Lot.Product != null ? s.Lot.Product.ProductName : null
                })
                .ToDictionaryAsync(x => x.Id, x => x);

            ViewBag.Recent = recentYm.Select(x => new ConvRecentVM {
                SerialId     = x.Id,
                LotId        = x.LotId,
                Tarih        = x.ArrivalDate,
                Parti        = x.PartiNo,
                Hedef        = x.Hedef,
                FscType      = x.FscType,
                Kg           = x.InitialWeight,
                Kalan        = x.CurrentWeight,
                Fire         = x.ConversionFireKg ?? 0m,
                KaynakSerialId = x.SourceSerialId,
                KaynakSerial = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].SerialNo : "—",
                KaynakKod    = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].Kod : null,
                KaynakAd     = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].Ad : "—"
            }).ToList();

            return View();
        }

        // POST /Conversion/Convert
        [HttpPost]
        public async Task<IActionResult> Convert(int sourceSerialId, int targetProductId,
                                                  decimal producedKg, decimal consumedKg,
                                                  DateTime? date, string? notes)
        {
            try
            {
                if (producedKg <= 0)
                    return Json(new { success = false, message = "Üretilen miktar sıfırdan büyük olmalıdır." });
                if (consumedKg <= 0)
                    return Json(new { success = false, message = "Tüketilen ham miktar sıfırdan büyük olmalıdır." });
                if (producedKg > consumedKg)
                    return Json(new { success = false, message = "Üretilen miktar, tüketilen ham miktarı aşamaz." });

                var source = await _context.FscSerials
                    .Include(s => s.Lot).ThenInclude(l => l.Product)
                    .FirstOrDefaultAsync(s => s.Id == sourceSerialId);
                if (source == null)
                    return Json(new { success = false, message = "Kaynak bobin bulunamadı." });
                if (consumedKg > source.CurrentWeight)
                    return Json(new { success = false, message = $"Tüketim ({consumedKg:N2} kg), kaynak bobinin kalanını ({source.CurrentWeight:N2} kg) aşıyor." });

                var target = await _context.Products.FindAsync(targetProductId);
                if (target == null)
                    return Json(new { success = false, message = "Hedef yarı mamül bulunamadı." });

                var when = (date ?? DateTime.Now).Date;
                var user = User.Identity?.Name ?? "System";

                // Parti no: YM{yy}-{sıra}
                var ymCount = await _context.FscLots.CountAsync(l => l.PartiNo.StartsWith("YM"));
                var partiNo = $"YM{when:yy}-{ymCount + 1:D3}";

                // 1) Kaynak bobinden düş
                source.CurrentWeight -= consumedKg;

                // 2) Hedef için yeni lot (FSC tipi kaynaktan devralınır — CoC)
                var srcName = source.Lot.Product?.ProductName ?? source.Lot.PartiNo;
                var lot = new FscLot
                {
                    PartiNo          = partiNo,
                    FscTypeId        = source.Lot.FscTypeId,
                    SupplierId       = null,
                    ProductId        = targetProductId,
                    ArrivalDate      = when,
                    Currency         = "TRY",
                    SourceSerialId   = source.Id,
                    ConversionFireKg = consumedKg - producedKg,
                    Notes       = $"Yarı mamül dönüşüm — kaynak: {srcName} / {source.SerialNo} ({consumedKg:N2} kg → {producedKg:N2} kg)"
                                  + (string.IsNullOrWhiteSpace(notes) ? "" : $" · {notes.Trim()}"),
                    CreatedBy   = user,
                    CreatedDate = DateTime.Now
                };
                _context.FscLots.Add(lot);
                await _context.SaveChangesAsync();

                // 3) Hedef bobin
                var serial = new FscSerial
                {
                    LotId         = lot.Id,
                    SerialNo      = $"{partiNo}-B01",
                    InitialWeight = producedKg,
                    CurrentWeight = producedKg,
                    Notes         = $"Dönüşüm çıktısı (fire {(consumedKg - producedKg):N2} kg)",
                    CreatedBy     = user,
                    CreatedDate   = DateTime.Now
                };
                _context.FscSerials.Add(serial);

                // 4) Stok hareketi — yarı mamül girişi
                _context.StockMovements.Add(new StockMovement
                {
                    Type         = MovementType.ProductionEntry,
                    ProductId    = targetProductId,
                    Quantity     = producedKg,
                    Unit         = "kg",
                    DocumentNo   = partiNo,
                    DocumentDate = when,
                    Description  = $"Yarı mamül dönüşüm: {srcName} → {target.ProductName}",
                    CreatedBy    = user,
                    CreatedDate  = DateTime.Now
                });

                // 4b) Stok hareketi — kaynak ham/YM tüketimi (çıkış)
                if (source.Lot.ProductId.HasValue)
                {
                    _context.StockMovements.Add(new StockMovement
                    {
                        Type         = MovementType.ProductionConsumption,
                        ProductId    = source.Lot.ProductId.Value,
                        Quantity     = consumedKg,
                        Unit         = "kg",
                        DocumentNo   = partiNo,
                        DocumentDate = when,
                        Description  = $"Dönüşüm tüketimi: {srcName} → {target.ProductName}",
                        CreatedBy    = user,
                        CreatedDate  = DateTime.Now
                    });
                }

                // 5) Fire → Fire Raporu (WasteManagement) — tüketilen ile üretilen farkı
                var fire = consumedKg - producedKg;
                if (fire > 0)
                {
                    var atkCount = await _context.WasteManagements.CountAsync(w => w.CreatedDate.Year == DateTime.Now.Year);
                    _context.WasteManagements.Add(new WasteManagement
                    {
                        WasteCode      = $"ATK{DateTime.Now.Year}-{atkCount + 1:D3}",
                        Category       = WasteCategory.KesimArtigi,
                        Description    = $"Yarı mamül dönüşüm firesi: {srcName} → {target.ProductName}",
                        Quantity       = fire,
                        Unit           = "kg",
                        DisposalDate   = when,
                        DisposalMethod = "Geri Dönüşüm",
                        Notes          = $"Dönüşüm parti {partiNo}",
                        CreatedBy      = user,
                        CreatedDate    = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Dönüşüm tamam: {producedKg:N2} kg {target.ProductName} üretildi (Parti {partiNo}).",
                    partiNo,
                    kalanKaynak = source.CurrentWeight
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Conversion/GetConversion/{serialId} — düzenleme için mevcut veri
        [HttpGet]
        public async Task<IActionResult> GetConversion(int serialId)
        {
            var s = await _context.FscSerials
                .Include(x => x.Lot).ThenInclude(l => l.Product)
                .Include(x => x.Lot).ThenInclude(l => l.FscType)
                .FirstOrDefaultAsync(x => x.Id == serialId);
            if (s == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

            return Json(new { success = true, data = new {
                serialId  = s.Id,
                lotId     = s.LotId,
                tarih     = s.Lot.ArrivalDate.ToString("yyyy-MM-dd"),
                parti     = s.Lot.PartiNo,
                hedef     = s.Lot.Product?.ProductName,
                kg        = s.InitialWeight,
                fire      = s.Lot.ConversionFireKg ?? 0m,
                sourceSerialId = s.Lot.SourceSerialId
            }});
        }

        // POST /Conversion/UpdateConversion — tarih ve fire güncelle
        [HttpPost]
        public async Task<IActionResult> UpdateConversion(int serialId, DateTime tarih, decimal fire, string? notes)
        {
            try
            {
                var s = await _context.FscSerials
                    .Include(x => x.Lot)
                    .FirstOrDefaultAsync(x => x.Id == serialId);
                if (s == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                var lot = s.Lot;
                lot.ArrivalDate       = tarih;
                lot.ConversionFireKg  = fire >= 0 ? fire : lot.ConversionFireKg;
                if (!string.IsNullOrWhiteSpace(notes)) lot.Notes = notes.Trim();

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Dönüşüm kaydı güncellendi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // GET /Conversion/ExportExcel?parti=YM25-001&hedef=...&fsc=...&tarih=yyyy-MM-dd
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string[]? parti, string[]? hedef, string[]? fsc, string? tarih)
        {
            var query = _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Where(s => s.Lot.PartiNo.StartsWith("YM"))
                .AsQueryable();

            if (parti?.Length > 0)
                query = query.Where(s => parti.Contains(s.Lot.PartiNo));
            if (hedef?.Length > 0)
                query = query.Where(s => s.Lot.Product != null && hedef.Contains(s.Lot.Product.ProductName));
            if (fsc?.Length > 0)
                query = query.Where(s => fsc.Contains(s.Lot.FscType.Name));
            if (!string.IsNullOrWhiteSpace(tarih) && DateTime.TryParse(tarih, out var t))
                query = query.Where(s => s.Lot.ArrivalDate.Date == t.Date);

            var recentYm = await query
                .OrderByDescending(s => s.Id)
                .Select(s => new {
                    s.Id, LotId = s.LotId,
                    s.Lot.ArrivalDate, s.Lot.PartiNo,
                    Hedef    = s.Lot.Product != null ? s.Lot.Product.ProductName : "—",
                    FscType  = s.Lot.FscType.Name,
                    s.InitialWeight, s.CurrentWeight,
                    s.Lot.SourceSerialId, s.Lot.ConversionFireKg
                })
                .ToListAsync();

            var srcIds = recentYm.Where(x => x.SourceSerialId != null).Select(x => x.SourceSerialId!.Value).Distinct().ToList();
            var srcMap = await _context.FscSerials
                .Where(s => srcIds.Contains(s.Id))
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .Select(s => new {
                    s.Id, s.SerialNo,
                    Kod = s.Lot.Product != null ? s.Lot.Product.ExternalCode : null,
                    Ad  = s.Lot.Product != null ? s.Lot.Product.ProductName  : null
                })
                .ToDictionaryAsync(x => x.Id, x => x);

            var rows = recentYm.Select(x => new {
                Tarih        = x.ArrivalDate.ToString("dd.MM.yyyy"),
                Parti        = x.PartiNo,
                KaynakKod    = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].Kod ?? "" : "",
                KaynakAd     = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].Ad  ?? "—" : "—",
                KaynakSerial = x.SourceSerialId != null && srcMap.ContainsKey(x.SourceSerialId.Value) ? srcMap[x.SourceSerialId.Value].SerialNo : "—",
                HedefYm      = x.Hedef,
                FscType      = x.FscType,
                UretilenKg   = x.InitialWeight,
                FireKg       = x.ConversionFireKg ?? 0m,
                KalanKg      = x.CurrentWeight
            }).ToList();

            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("YM Dönüşümler");

            string[] headers = ["Tarih", "Parti", "Kaynak Kod", "Kaynak Ürün", "Kaynak Seri No", "Hedef YM", "FSC Tipi", "Üretilen (kg)", "Fire (kg)", "Kalan (kg)"];
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#1e3d14");
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            for (int r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                int ri = r + 2;
                ws.Cell(ri, 1).Value  = row.Tarih;
                ws.Cell(ri, 2).Value  = row.Parti;
                ws.Cell(ri, 3).Value  = row.KaynakKod;
                ws.Cell(ri, 4).Value  = row.KaynakAd;
                ws.Cell(ri, 5).Value  = row.KaynakSerial;
                ws.Cell(ri, 6).Value  = row.HedefYm;
                ws.Cell(ri, 7).Value  = row.FscType;
                ws.Cell(ri, 8).Value  = row.UretilenKg;
                ws.Cell(ri, 9).Value  = row.FireKg;
                ws.Cell(ri, 10).Value = row.KalanKg;
                if (r % 2 == 1)
                    ws.Row(ri).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#f9fafb");
            }

            ws.Columns().AdjustToContents();

            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;
            var fileName = $"YM_Donusumler_{DateTime.Now:ddMMyyyy}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // POST /Conversion/DeleteConversion — dönüşümü geri al ve sil
        [HttpPost]
        public async Task<IActionResult> DeleteConversion(int serialId)
        {
            try
            {
                var s = await _context.FscSerials
                    .Include(x => x.Lot).ThenInclude(l => l.Serials)
                    .FirstOrDefaultAsync(x => x.Id == serialId);
                if (s == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                var lot = s.Lot;

                // Kaynak bobini geri yükle (dönüşüm öncesi ağırlık)
                if (lot.SourceSerialId.HasValue)
                {
                    var src = await _context.FscSerials
                        .Include(x => x.Lot)
                        .FirstOrDefaultAsync(x => x.Id == lot.SourceSerialId.Value);
                    if (src != null)
                    {
                        // Tüketilen miktarı geri ekle: src.CurrentWeight += s.InitialWeight + (fire)
                        var consumed = s.InitialWeight + (lot.ConversionFireKg ?? 0m);
                        src.CurrentWeight += consumed;

                        // İlgili StockMovement çıkış kaydını sil (ErpReferenceId veya DocumentNo ile eşleştir)
                        var sm = await _context.StockMovements
                            .FirstOrDefaultAsync(m => m.Type == MovementType.ProductionConsumption
                                && m.ProductId == src.Lot.ProductId
                                && m.DocumentNo != null && m.DocumentNo.Contains(lot.PartiNo));
                        if (sm != null) _context.StockMovements.Remove(sm);
                    }
                }

                // YM lot'unu ve seri(ler)ini sil
                _context.FscSerials.RemoveRange(lot.Serials);
                _context.FscLots.Remove(lot);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Dönüşüm {lot.PartiNo} silindi ve kaynak bobin geri yüklendi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }

    // ── View modelleri (dynamic ViewBag yerine public tipler) ──────────────
    public class ConvSourceVM
    {
        public int Id { get; set; }
        public string SerialNo { get; set; } = "";
        public decimal CurrentWeight { get; set; }
        public string FscType { get; set; } = "";
        public string? UrunKod { get; set; }      // ExternalCode (dış kod)
        public string UrunAd { get; set; } = "";
        public string? UrunIcKod { get; set; }    // ProductCode (iç stok kodu)
        public string? PartiNo { get; set; }      // Lot.PartiNo
    }

    public class ConvTargetVM
    {
        public int Id { get; set; }
        public string Kod { get; set; } = "";
        public string Ad { get; set; } = "";
        public string? ExtKod { get; set; }       // ExternalCode
    }

    public class ConvRecentVM
    {
        public int SerialId { get; set; }     // FscSerial.Id — düzenleme/silme için
        public int LotId    { get; set; }     // FscLot.Id
        public DateTime Tarih { get; set; }
        public string Parti { get; set; } = "";
        public string Hedef { get; set; } = "";
        public string FscType { get; set; } = "";
        public decimal Kg { get; set; }
        public decimal Kalan { get; set; }
        public decimal Fire { get; set; }
        public string KaynakSerial { get; set; } = "—";
        public string? KaynakKod { get; set; }
        public string? KaynakAd { get; set; }
        public int? KaynakSerialId { get; set; }  // geri-alma için kaynak serial id
    }
}
