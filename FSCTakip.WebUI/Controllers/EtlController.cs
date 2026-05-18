using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FSCTakip.WebUI.Controllers
{
    public class EtlController : BaseController
    {
        private readonly IConfiguration _cfg;
        public EtlController(AppDbContext context, IConfiguration cfg) : base(context) { _cfg = cfg; }

        // ─── Index — Dashboard ────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "ERP Entegrasyonu";
            ViewBag.TotalConnections  = await _context.EtlConnections.CountAsync();
            ViewBag.ActiveConnections = await _context.EtlConnections.CountAsync(c => c.IsActive);
            ViewBag.TotalJobs         = await _context.EtlJobs.CountAsync();
            ViewBag.SuccessJobs       = await _context.EtlJobs.CountAsync(j => j.Status == "Completed");
            ViewBag.RecentJobs        = await _context.EtlJobs
                .Include(j => j.EtlConnection)
                .OrderByDescending(j => j.StartedAt)
                .Take(5)
                .ToListAsync();
            return View();
        }

        // ─── Bağlantı Yönetimi ────────────────────────────────────────────────
        public async Task<IActionResult> Connections()
        {
            ViewData["Title"] = "Bağlantı Yönetimi";
            var list = await _context.EtlConnections.OrderByDescending(c => c.CreatedDate).ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetConnection(int id)
        {
            var c = await _context.EtlConnections.FindAsync(id);
            if (c == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                c.Id, c.Name, c.Type, c.Description, c.Settings, c.IsActive
            }});
        }

        [HttpPost]
        public async Task<IActionResult> SaveConnection(EtlConnection model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, message = "Bağlantı adı zorunludur." });

            if (model.Id == 0)
            {
                model.IsActive    = true;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy   = "SYSTEM";
                _context.EtlConnections.Add(model);
            }
            else
            {
                var existing = await _context.EtlConnections.FindAsync(model.Id);
                if (existing == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
                existing.Name        = model.Name;
                existing.Type        = model.Type;
                existing.Description = model.Description;
                existing.Settings    = model.Settings;
                existing.UpdatedDate = DateTime.Now;
                existing.UpdatedBy   = "SYSTEM";
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleConnection(int id)
        {
            var c = await _context.EtlConnections.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            c.IsActive    = !c.IsActive;
            c.UpdatedDate = DateTime.Now;
            c.UpdatedBy   = "SYSTEM";
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = c.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConnection(int id)
        {
            var hasJobs = await _context.EtlJobs.AnyAsync(j => j.EtlConnectionId == id);
            if (hasJobs) return Json(new { success = false, message = "Bu bağlantıya ait aktarım geçmişi mevcut, silinemez." });
            var c = await _context.EtlConnections.FindAsync(id);
            if (c == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.EtlConnections.Remove(c);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ─── Aktarım Geçmişi ─────────────────────────────────────────────────
        public async Task<IActionResult> History()
        {
            ViewData["Title"] = "Aktarım Geçmişi";
            var list = await _context.EtlJobs
                .Include(j => j.EtlConnection)
                .OrderByDescending(j => j.StartedAt)
                .ToListAsync();
            return View(list);
        }

        // ─── Excel Aktarımı ───────────────────────────────────────────────────
        public async Task<IActionResult> Import()
        {
            ViewData["Title"] = "Excel Aktarımı";
            ViewBag.Connections = await _context.EtlConnections.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportPreview(IFormFile file, string jobType)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return Json(new { success = false, message = "Yalnızca Excel dosyası (.xlsx) kabul edilir." });

            try
            {
                using var stream = file.OpenReadStream();
                using var wb     = new XLWorkbook(stream);
                var ws = wb.Worksheets.First();

                var rows    = ws.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();
                var headers = ws.Row(1).CellsUsed().Select(c => c.GetString().Trim()).ToList();
                var preview = rows.Take(10).Select(r =>
                    r.Cells().Select(c => c.GetString()).ToList()
                ).ToList();

                return Json(new { success = true, headers, preview, totalRows = rows.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dosya okunamadı: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExecute(IFormFile file, string jobType, int? connectionId)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Dosya seçilmedi." });

            var job = new EtlJob
            {
                EtlConnectionId = connectionId,
                JobType         = jobType,
                Source          = "Manuel Excel",
                Status          = "Running",
                StartedAt       = DateTime.Now,
                SourceFile      = file.FileName,
                CreatedDate     = DateTime.Now,
                CreatedBy       = "SYSTEM"
            };
            _context.EtlJobs.Add(job);
            await _context.SaveChangesAsync();

            var errors = new List<string>();
            int inserted = 0, updated = 0, skipped = 0;

            try
            {
                using var stream = file.OpenReadStream();
                using var wb     = new XLWorkbook(stream);
                var ws   = wb.Worksheets.First();
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();

                job.TotalRecords = rows.Count;

                if (jobType == "ProductImport")
                    (inserted, updated, skipped, errors) = await ImportProducts(rows);
                else if (jobType == "SupplierImport")
                    (inserted, updated, skipped, errors) = await ImportSuppliers(rows);
                else if (jobType == "CustomerImport")
                    (inserted, updated, skipped, errors) = await ImportCustomers(rows);

                job.Status        = errors.Count == 0 ? "Completed" : (inserted + updated > 0 ? "Partial" : "Failed");
                job.InsertedCount = inserted;
                job.UpdatedCount  = updated;
                job.SkippedCount  = skipped;
                job.ErrorCount    = errors.Count;
                job.CompletedAt   = DateTime.Now;
                job.ErrorDetails  = errors.Count > 0 ? string.Join("\n", errors) : null;

                if (connectionId.HasValue)
                {
                    var conn = await _context.EtlConnections.FindAsync(connectionId.Value);
                    if (conn != null)
                    {
                        conn.LastSyncAt     = DateTime.Now;
                        conn.LastSyncStatus = job.Status;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, inserted, updated, skipped, errorCount = errors.Count, errors, jobId = job.Id });
            }
            catch (Exception ex)
            {
                job.Status        = "Failed";
                job.ErrorDetails  = ex.Message;
                job.CompletedAt   = DateTime.Now;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── Özel indir şablonları ────────────────────────────────────────────
        public IActionResult DownloadTemplate(string type)
        {
            using var wb = new XLWorkbook();
            IXLWorksheet ws;

            if (type == "ProductImport")
            {
                ws = wb.AddWorksheet("Urunler");
                string[] cols = { "UrunKodu", "UrunAdi", "Birim", "GrupAdi", "IsActive" };
                for (int i = 0; i < cols.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = cols[i];
                    ws.Cell(1, i + 1).Style.Font.Bold = true;
                }
                ws.Cell(2, 1).Value = "HM-999"; ws.Cell(2, 2).Value = "Örnek Hammadde"; ws.Cell(2, 3).Value = "Kg";
                ws.Cell(2, 4).Value = "Hammadde"; ws.Cell(2, 5).Value = "1";
            }
            else if (type == "SupplierImport")
            {
                ws = wb.AddWorksheet("Tedarikciler");
                string[] cols = { "TedarikciKodu", "TedarikciAdi", "FscKodu", "ContactPerson", "Telefon", "Email" };
                for (int i = 0; i < cols.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = cols[i];
                    ws.Cell(1, i + 1).Style.Font.Bold = true;
                }
                ws.Cell(2, 1).Value = "TED-999"; ws.Cell(2, 2).Value = "Örnek Tedarikçi A.Ş.";
                ws.Cell(2, 3).Value = "FSC-C000001"; ws.Cell(2, 4).Value = "İletişim Kişisi";
                ws.Cell(2, 5).Value = "02121234567"; ws.Cell(2, 6).Value = "info@ornek.com";
            }
            else // CustomerImport
            {
                ws = wb.AddWorksheet("Musteriler");
                string[] cols = { "MusteriKodu", "MusteriAdi", "VergiNo", "VergiDairesi", "Sehir", "Telefon", "Email" };
                for (int i = 0; i < cols.Length; i++)
                {
                    ws.Cell(1, i + 1).Value = cols[i];
                    ws.Cell(1, i + 1).Style.Font.Bold = true;
                }
                ws.Cell(2, 1).Value = "MHS-999"; ws.Cell(2, 2).Value = "Örnek Müşteri Ltd.";
                ws.Cell(2, 3).Value = "1234567890"; ws.Cell(2, 4).Value = "Kadıköy";
                ws.Cell(2, 5).Value = "İstanbul"; ws.Cell(2, 6).Value = "02161234567";
                ws.Cell(2, 7).Value = "info@musteri.com";
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Sablon_{type}_{DateTime.Now:ddMMyyyy}.xlsx");
        }

        // ─── Netsis Senkronizasyon ────────────────────────────────────────────
        public async Task<IActionResult> NetsisSync()
        {
            ViewData["Title"] = "Netsis Senkronizasyonu";
            ViewBag.Connections = await _context.EtlConnections
                .Where(c => c.IsActive && c.Type == "Netsis")
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.RecentJobs = await _context.EtlJobs
                .Where(j => j.Source == "Netsis")
                .Include(j => j.EtlConnection)
                .OrderByDescending(j => j.StartedAt)
                .Take(10)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NetsisExecute(string syncType, int? connectionId)
        {
            var connStr = _cfg.GetConnectionString("NetsisConnection");

            if (connectionId.HasValue)
            {
                var etlConn = await _context.EtlConnections.FindAsync(connectionId.Value);
                if (etlConn?.Settings != null)
                {
                    try
                    {
                        var s = JsonSerializer.Deserialize<Dictionary<string, string>>(etlConn.Settings);
                        if (s != null && s.ContainsKey("Server") && s.ContainsKey("Database"))
                        {
                            var b = new SqlConnectionStringBuilder
                            {
                                DataSource         = s.GetValueOrDefault("Server", ""),
                                InitialCatalog     = s.GetValueOrDefault("Database", ""),
                                UserID             = s.GetValueOrDefault("UserId", ""),
                                Password           = s.GetValueOrDefault("Password", ""),
                                TrustServerCertificate = true
                            };
                            connStr = b.ConnectionString;
                        }
                    }
                    catch { }
                }
            }

            if (string.IsNullOrWhiteSpace(connStr))
                return Json(new { success = false, message = "Netsis bağlantı dizgisi tanımlı değil." });

            var job = new EtlJob
            {
                EtlConnectionId = connectionId,
                JobType         = syncType,
                Source          = "Netsis",
                Status          = "Running",
                StartedAt       = DateTime.Now,
                CreatedDate     = DateTime.Now,
                CreatedBy       = "SYSTEM"
            };
            _context.EtlJobs.Add(job);
            await _context.SaveChangesAsync();

            int inserted = 0, updated = 0, skipped = 0;
            var errors = new List<string>();

            try
            {
                using var netsisCon = new SqlConnection(connStr);
                await netsisCon.OpenAsync();

                (inserted, updated, skipped, errors) = syncType switch
                {
                    "ProductImport"  => await SyncNetsisProducts(netsisCon),
                    "SupplierImport" => await SyncNetsisSuppliers(netsisCon),
                    "CustomerImport" => await SyncNetsisCustomers(netsisCon),
                    _ => (0, 0, 0, new List<string> { "Bilinmeyen senkronizasyon türü." })
                };

                job.Status        = errors.Count == 0 ? "Completed" : (inserted + updated > 0 ? "Partial" : "Failed");
                job.InsertedCount = inserted;
                job.UpdatedCount  = updated;
                job.SkippedCount  = skipped;
                job.ErrorCount    = errors.Count;
                job.CompletedAt   = DateTime.Now;
                job.ErrorDetails  = errors.Count > 0 ? string.Join("\n", errors.Take(50)) : null;

                if (connectionId.HasValue)
                {
                    var conn = await _context.EtlConnections.FindAsync(connectionId.Value);
                    if (conn != null) { conn.LastSyncAt = DateTime.Now; conn.LastSyncStatus = job.Status; }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, inserted, updated, skipped, errorCount = errors.Count, errors = errors.Take(20), jobId = job.Id });
            }
            catch (Exception ex)
            {
                job.Status       = "Failed";
                job.ErrorDetails = ex.Message;
                job.CompletedAt  = DateTime.Now;
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── Netsis veri çekme yardımcıları ──────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> SyncNetsisProducts(SqlConnection cn)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var groups = await _context.ProductGroups.ToListAsync();

            const string sql = @"
                SELECT STOK_KODU, STOK_ADI, OLCU_BR1, GRUP_KODU, MAMULMU
                FROM TBLSTSABIT
                WHERE GRUP_KODU IN (10, 20, 30, 40, 50)
                ORDER BY STOK_KODU";

            using var cmd = new SqlCommand(sql, cn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var rows = new List<(string Kod, string Adi, string Birim, int Grup, bool Mamul)>();
            while (await rdr.ReadAsync())
            {
                rows.Add((
                    rdr["STOK_KODU"]?.ToString()?.Trim() ?? "",
                    rdr["STOK_ADI"]?.ToString()?.Trim() ?? "",
                    rdr["OLCU_BR1"]?.ToString()?.Trim() ?? "AD",
                    Convert.ToInt32(rdr["GRUP_KODU"]),
                    rdr["MAMULMU"]?.ToString() == "1"
                ));
            }
            rdr.Close();

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Kod) || string.IsNullOrWhiteSpace(r.Adi)) { skp++; continue; }
                try
                {
                    var grupAdi = r.Grup switch { 10 => "Hammadde", 20 => "Sap", 30 => "Mamul", 40 => "Kimyasal", _ => "Sarf" };
                    var group   = groups.FirstOrDefault(g => g.GroupName.Equals(grupAdi, StringComparison.OrdinalIgnoreCase));

                    var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == r.Kod);
                    if (existing == null)
                    {
                        _context.Products.Add(new Product
                        {
                            ProductCode    = r.Kod,
                            ProductName    = r.Adi,
                            Unit           = r.Birim.ToUpperInvariant(),
                            ProductGroupId = group?.Id,
                            IsActive       = true,
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "NETSIS"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.ProductName    = r.Adi;
                        existing.Unit           = r.Birim.ToUpperInvariant();
                        existing.ProductGroupId = group?.Id ?? existing.ProductGroupId;
                        existing.UpdatedDate    = DateTime.Now;
                        existing.UpdatedBy      = "NETSIS";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"{r.Kod}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> SyncNetsisSuppliers(SqlConnection cn)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count = await _context.Suppliers.CountAsync();

            const string sql = @"
                SELECT CARI_KOD, CARI_ISIM, CARI_TEL, EMAIL, VERGI_NUMARASI, VERGI_DAIRESI, CARI_IL
                FROM TBLCASABIT
                WHERE CARI_TIP = 'S'
                ORDER BY CARI_KOD";

            using var cmd = new SqlCommand(sql, cn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var rows = new List<(string Kod, string Isim, string Tel, string Mail, string Vn, string Vd, string Il)>();
            while (await rdr.ReadAsync())
            {
                rows.Add((
                    rdr["CARI_KOD"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_ISIM"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_TEL"]?.ToString()?.Trim() ?? "",
                    rdr["EMAIL"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_NUMARASI"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_DAIRESI"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_IL"]?.ToString()?.Trim() ?? ""
                ));
            }
            rdr.Close();

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Isim)) { skp++; continue; }
                try
                {
                    var existing = await _context.Suppliers
                        .FirstOrDefaultAsync(s => s.SupplierCode == r.Kod || s.Name == r.Isim);

                    var email = NormalizeEmail(r.Mail);

                    if (existing == null)
                    {
                        count++;
                        _context.Suppliers.Add(new Supplier
                        {
                            SupplierCode  = string.IsNullOrWhiteSpace(r.Kod) ? $"TED-{count:D3}" : r.Kod,
                            Name          = r.Isim,
                            Phone         = r.Tel,
                            Email         = email,
                            IsActive      = true,
                            IsFscActive   = false,
                            CreatedDate   = DateTime.Now,
                            CreatedBy     = "NETSIS"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name        = r.Isim;
                        existing.Phone       = r.Tel;
                        existing.Email       = email;
                        existing.UpdatedDate = DateTime.Now;
                        existing.UpdatedBy   = "NETSIS";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"{r.Kod} - {r.Isim}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> SyncNetsisCustomers(SqlConnection cn)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count = await _context.Customers.CountAsync();

            const string sql = @"
                SELECT CARI_KOD, CARI_ISIM, CARI_TEL, EMAIL, VERGI_NUMARASI, VERGI_DAIRESI, CARI_IL
                FROM TBLCASABIT
                WHERE CARI_TIP = 'A'
                ORDER BY CARI_KOD";

            using var cmd = new SqlCommand(sql, cn);
            using var rdr = await cmd.ExecuteReaderAsync();

            var rows = new List<(string Kod, string Isim, string Tel, string Mail, string Vn, string Vd, string Il)>();
            while (await rdr.ReadAsync())
            {
                rows.Add((
                    rdr["CARI_KOD"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_ISIM"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_TEL"]?.ToString()?.Trim() ?? "",
                    rdr["EMAIL"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_NUMARASI"]?.ToString()?.Trim() ?? "",
                    rdr["VERGI_DAIRESI"]?.ToString()?.Trim() ?? "",
                    rdr["CARI_IL"]?.ToString()?.Trim() ?? ""
                ));
            }
            rdr.Close();

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Isim)) { skp++; continue; }
                try
                {
                    var existing = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerCode == r.Kod || c.Name == r.Isim);

                    var email = NormalizeEmail(r.Mail);

                    if (existing == null)
                    {
                        count++;
                        _context.Customers.Add(new Customer
                        {
                            CustomerCode = string.IsNullOrWhiteSpace(r.Kod) ? $"MHS-{count:D3}" : r.Kod,
                            Name         = r.Isim,
                            Phone        = r.Tel,
                            Email        = email,
                            TaxNumber    = r.Vn,
                            TaxOffice    = r.Vd,
                            City         = r.Il,
                            IsActive     = true,
                            CreatedDate  = DateTime.Now,
                            CreatedBy    = "NETSIS"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name        = r.Isim;
                        existing.Phone       = r.Tel;
                        existing.Email       = email;
                        existing.TaxNumber   = r.Vn;
                        existing.TaxOffice   = r.Vd;
                        existing.City        = r.Il;
                        existing.UpdatedDate = DateTime.Now;
                        existing.UpdatedBy   = "NETSIS";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"{r.Kod} - {r.Isim}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            return email.Replace("İ", "i").Replace("I", "ı").ToLowerInvariant();
        }

        // ─── Import yardımcıları ──────────────────────────────────────────────
        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportProducts(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var groups = await _context.ProductGroups.ToListAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var unit      = row.Cell(3).GetString().Trim().IfEmpty("Adet");
                    var groupName = row.Cell(4).GetString().Trim();
                    var isActive  = row.Cell(5).GetString().Trim() != "0";
                    var group     = groups.FirstOrDefault(g => g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                    var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductCode == code);
                    if (existing == null)
                    {
                        _context.Products.Add(new Product
                        {
                            ProductCode    = code,
                            ProductName    = name,
                            Unit           = unit,
                            ProductGroupId = group?.Id,
                            IsActive       = isActive,
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "ETL"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.ProductName    = name;
                        existing.Unit           = unit;
                        existing.ProductGroupId = group?.Id ?? existing.ProductGroupId;
                        existing.IsActive       = isActive;
                        existing.UpdatedDate    = DateTime.Now;
                        existing.UpdatedBy      = "ETL";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"Satır {i + 2}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportSuppliers(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count  = await _context.Suppliers.CountAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var fscCode = row.Cell(3).GetString().Trim();
                    var contact = row.Cell(4).GetString().Trim();
                    var phone   = row.Cell(5).GetString().Trim();
                    var email   = row.Cell(6).GetString().Trim();

                    var existing = string.IsNullOrWhiteSpace(code)
                        ? null
                        : await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == code);

                    if (existing == null)
                    {
                        count++;
                        _context.Suppliers.Add(new Supplier
                        {
                            SupplierCode   = string.IsNullOrWhiteSpace(code) ? $"TED-{count:D3}" : code,
                            Name           = name,
                            FscCode        = fscCode,
                            ContactPerson  = contact,
                            Phone          = phone,
                            Email          = email.ToLowerInvariant(),
                            IsActive       = true,
                            IsFscActive    = !string.IsNullOrWhiteSpace(fscCode),
                            CreatedDate    = DateTime.Now,
                            CreatedBy      = "ETL"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name          = name;
                        existing.FscCode       = fscCode;
                        existing.ContactPerson = contact;
                        existing.Phone         = phone;
                        existing.Email         = email.ToLowerInvariant();
                        existing.UpdatedDate   = DateTime.Now;
                        existing.UpdatedBy     = "ETL";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"Satır {i + 2}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }

        private async Task<(int ins, int upd, int skp, List<string> errs)> ImportCustomers(List<IXLRangeRow> rows)
        {
            int ins = 0, upd = 0, skp = 0;
            var errors = new List<string>();
            var count  = await _context.Customers.CountAsync();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                try
                {
                    var code = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(name)) { skp++; continue; }

                    var taxNo     = row.Cell(3).GetString().Trim();
                    var taxOffice = row.Cell(4).GetString().Trim();
                    var city      = row.Cell(5).GetString().Trim();
                    var phone     = row.Cell(6).GetString().Trim();
                    var email     = row.Cell(7).GetString().Trim();

                    var existing = string.IsNullOrWhiteSpace(code)
                        ? null
                        : await _context.Customers.FirstOrDefaultAsync(c => c.CustomerCode == code);

                    if (existing == null)
                    {
                        count++;
                        _context.Customers.Add(new Customer
                        {
                            CustomerCode = string.IsNullOrWhiteSpace(code) ? $"MHS-{count:D3}" : code,
                            Name         = name,
                            TaxNumber    = taxNo,
                            TaxOffice    = taxOffice,
                            City         = city,
                            Phone        = phone,
                            Email        = email.ToLowerInvariant(),
                            IsActive     = true,
                            CreatedDate  = DateTime.Now,
                            CreatedBy    = "ETL"
                        });
                        ins++;
                    }
                    else
                    {
                        existing.Name      = name;
                        existing.TaxNumber = taxNo;
                        existing.TaxOffice = taxOffice;
                        existing.City      = city;
                        existing.Phone     = phone;
                        existing.Email     = email.ToLowerInvariant();
                        existing.UpdatedDate = DateTime.Now;
                        existing.UpdatedBy   = "ETL";
                        upd++;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { errors.Add($"Satır {i + 2}: {ex.Message}"); }
            }
            return (ins, upd, skp, errors);
        }
    }

    internal static class StringExt
    {
        public static string IfEmpty(this string s, string fallback) =>
            string.IsNullOrWhiteSpace(s) ? fallback : s;
    }
}
