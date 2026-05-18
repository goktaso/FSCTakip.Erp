using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class ProductionController : BaseController
    {
        public ProductionController(AppDbContext context) : base(context) { }

        // GET /Production/Index
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? productId, WorkOrderStatus? status)
        {
            var query = _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Include(w => w.ProductionDetails)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(w => w.PlannedDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(w => w.PlannedDate <= endDate.Value.AddDays(1));
            if (productId.HasValue) query = query.Where(w => w.ProductId == productId.Value);
            if (status.HasValue)    query = query.Where(w => w.Status == status.Value);

            ViewBag.Products  = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.Machines  = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate   = endDate?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(w => w.Id).ToListAsync());
        }

        // GET /Production/GetWorkOrder/{id}
        [HttpGet]
        public async Task<IActionResult> GetWorkOrder(int id)
        {
            var w = await _context.WorkOrders.FindAsync(id);
            if (w == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                w.Id, w.WorkOrderNo, w.ProductId, w.MachineId,
                plannedDate = w.PlannedDate.ToString("yyyy-MM-dd"),
                w.PlannedQuantity, w.Notes, status = (int)w.Status
            }});
        }

        // POST /Production/SaveWorkOrder
        [HttpPost]
        public async Task<IActionResult> SaveWorkOrder(WorkOrder model)
        {
            try
            {
                if (model.Id == 0 && string.IsNullOrWhiteSpace(model.WorkOrderNo))
                {
                    var count = await _context.WorkOrders.CountAsync(w => w.CreatedDate.Year == DateTime.Now.Year);
                    model.WorkOrderNo = $"IE{DateTime.Now.Year}-{count + 1:D3}";
                }

                if (model.Id == 0)
                    _context.WorkOrders.Add(model);
                else
                {
                    var existing = await _context.WorkOrders.FindAsync(model.Id);
                    if (existing == null) return Json(new { success = false, message = "İş emri bulunamadı." });
                    existing.WorkOrderNo    = model.WorkOrderNo;
                    existing.ProductId      = model.ProductId;
                    existing.MachineId      = model.MachineId;
                    existing.PlannedDate    = model.PlannedDate;
                    existing.PlannedQuantity = model.PlannedQuantity;
                    existing.Notes          = model.Notes;
                    existing.Status         = model.Status;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "İş emri kaydedildi.", workOrderNo = model.WorkOrderNo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/DeleteWorkOrder
        [HttpPost]
        public async Task<IActionResult> DeleteWorkOrder(int id)
        {
            try
            {
                var wo = await _context.WorkOrders
                    .Include(w => w.ProductionDetails)
                    .FirstOrDefaultAsync(w => w.Id == id);

                if (wo == null) return Json(new { success = false, message = "İş emri bulunamadı." });
                if (wo.ProductionDetails.Any())
                    return Json(new { success = false, message = "Bu iş emrine ait üretim kaydı var, silinemez." });

                _context.WorkOrders.Remove(wo);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "İş emri silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/CompleteWorkOrder
        [HttpPost]
        public async Task<IActionResult> CompleteWorkOrder(int id)
        {
            try
            {
                var wo = await _context.WorkOrders.FindAsync(id);
                if (wo == null) return Json(new { success = false, message = "İş emri bulunamadı." });

                wo.Status        = WorkOrderStatus.Tamamlandi;
                wo.CompletedDate = DateTime.Now;
                wo.ActualQuantity = await _context.ProductionDetails
                    .Where(d => d.WorkOrderId == id)
                    .SumAsync(d => d.ProducedQuantity);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "İş emri tamamlandı olarak işaretlendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Production/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var wo = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.FscSerial)
                        .ThenInclude(s => s.Lot)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.Machine)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wo == null) return NotFound();

            // Hammadde bobinleri (kalan > 0 olanlar)
            var availableSerials = await _context.FscSerials
                .Include(s => s.Lot)
                    .ThenInclude(l => l.Supplier)
                .Where(s => s.CurrentWeight > 0)
                .OrderBy(s => s.SerialNo)
                .ToListAsync();

            ViewBag.AvailableSerials = availableSerials;
            ViewBag.Machines = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            ViewData["Title"] = $"İş Emri — {wo.WorkOrderNo}";
            return View(wo);
        }

        // GET /Production/GetDetail/{id}
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var d = await _context.ProductionDetails.FindAsync(id);
            if (d == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                d.Id, d.WorkOrderId, d.FscSerialId, d.MachineId,
                productionDate = d.ProductionDate.ToString("yyyy-MM-dd"),
                d.ConsumedWeight, d.WasteWeight, d.ProducedQuantity, d.Notes
            }});
        }

        // POST /Production/SaveDetail
        [HttpPost]
        public async Task<IActionResult> SaveDetail(ProductionDetail model)
        {
            try
            {
                var serial = await _context.FscSerials
                    .Include(s => s.ProductionDetails)
                    .FirstOrDefaultAsync(s => s.Id == model.FscSerialId);

                if (serial == null)
                    return Json(new { success = false, message = "Bobin bulunamadı." });

                if (model.Id == 0)
                {
                    // Yeni kayıt: stok düş
                    if (model.ConsumedWeight > serial.CurrentWeight)
                        return Json(new { success = false, message = $"Tüketim miktarı ({model.ConsumedWeight:N2} kg) bobinin kalan ağırlığını ({serial.CurrentWeight:N2} kg) aşıyor." });

                    serial.CurrentWeight -= model.ConsumedWeight;
                    _context.ProductionDetails.Add(model);
                }
                else
                {
                    var existing = await _context.ProductionDetails.FindAsync(model.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Kayıt bulunamadı." });

                    // Eski tüketimi iade et, yenisini düş
                    var diff = model.ConsumedWeight - existing.ConsumedWeight;
                    if (diff > serial.CurrentWeight)
                        return Json(new { success = false, message = $"Güncellenmiş tüketim bobinin kalan ağırlığını aşıyor." });

                    serial.CurrentWeight      -= diff;
                    existing.FscSerialId      = model.FscSerialId;
                    existing.MachineId        = model.MachineId;
                    existing.ProductionDate   = model.ProductionDate;
                    existing.ConsumedWeight   = model.ConsumedWeight;
                    existing.WasteWeight      = model.WasteWeight;
                    existing.ProducedQuantity = model.ProducedQuantity;
                    existing.Notes            = model.Notes;
                }

                // İş emrini "Üretimde" durumuna geçir
                var wo = await _context.WorkOrders.FindAsync(model.WorkOrderId);
                if (wo != null && wo.Status == WorkOrderStatus.Taslak)
                    wo.Status = WorkOrderStatus.Uretimde;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tüketim kaydedildi.", kalanKg = serial.CurrentWeight });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/DeleteDetail
        [HttpPost]
        public async Task<IActionResult> DeleteDetail(int id)
        {
            try
            {
                var detail = await _context.ProductionDetails
                    .Include(d => d.FscSerial)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (detail == null)
                    return Json(new { success = false, message = "Kayıt bulunamadı." });

                // Tüketimi iade et
                detail.FscSerial.CurrentWeight += detail.ConsumedWeight;

                _context.ProductionDetails.Remove(detail);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tüketim kaydı silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Production/ExportIndex
        public async Task<IActionResult> ExportIndex()
        {
            var list = await _context.WorkOrders
                .Include(w => w.Product).Include(w => w.Machine).Include(w => w.ProductionDetails)
                .OrderByDescending(w => w.Id).ToListAsync();

            var rows = list.Select(w => new {
                IsEmriNo        = w.WorkOrderNo,
                Urun            = w.Product?.ProductName,
                Makine          = w.Machine?.Name,
                PlanTarihi      = w.PlannedDate.ToString("dd.MM.yyyy"),
                BitisTargihi    = w.CompletedDate?.ToString("dd.MM.yyyy"),
                PlanAdet        = w.PlannedQuantity,
                GercekAdet      = w.ActualQuantity,
                Durum           = w.Status.ToString(),
                ToplamTuketimKg = w.ProductionDetails.Sum(d => d.ConsumedWeight),
                ToplamFireKg    = w.ProductionDetails.Sum(d => d.WasteWeight)
            });

            return ExportToExcel(rows, "IsEmirleri");
        }

        // GET /Production/WasteReport
        public async Task<IActionResult> WasteReport(DateTime? startDate, DateTime? endDate, int? machineId, int? productId)
        {
            var query = _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w.Product)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(d => d.Machine)
                .Where(d => d.WasteWeight > 0)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(d => d.ProductionDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(d => d.ProductionDate <= endDate.Value.AddDays(1));
            if (machineId.HasValue) query = query.Where(d => d.MachineId == machineId.Value);
            if (productId.HasValue) query = query.Where(d => d.WorkOrder.ProductId == productId.Value);

            var details = await query.OrderByDescending(d => d.ProductionDate).ToListAsync();

            // Makine bazlı özet
            var byMachine = details
                .GroupBy(d => new { d.MachineId, Name = d.Machine?.Name ?? "—" })
                .Select(g => new WasteGroupRow {
                    Label          = g.Key.Name,
                    RecordCount    = g.Count(),
                    TotalConsumed  = g.Sum(d => d.ConsumedWeight),
                    TotalWaste     = g.Sum(d => d.WasteWeight)
                })
                .OrderByDescending(r => r.WasteRate)
                .ToList();

            // Ürün bazlı özet
            var byProduct = details
                .GroupBy(d => new { Id = d.WorkOrder?.ProductId, Name = d.WorkOrder?.Product?.ProductName ?? "—" })
                .Select(g => new WasteGroupRow {
                    Label         = g.Key.Name,
                    RecordCount   = g.Count(),
                    TotalConsumed = g.Sum(d => d.ConsumedWeight),
                    TotalWaste    = g.Sum(d => d.WasteWeight)
                })
                .OrderByDescending(r => r.WasteRate)
                .ToList();

            // Aylık trend (son 6 ay)
            var monthly = details
                .GroupBy(d => new { d.ProductionDate.Year, d.ProductionDate.Month })
                .Select(g => new {
                    Label    = $"{g.Key.Year}/{g.Key.Month:D2}",
                    Consumed = g.Sum(d => d.ConsumedWeight),
                    Waste    = g.Sum(d => d.WasteWeight)
                })
                .OrderBy(r => r.Label)
                .TakeLast(6)
                .ToList();

            ViewBag.ByMachine  = byMachine;
            ViewBag.ByProduct  = byProduct;
            ViewBag.MonthlyLabels  = monthly.Select(m => m.Label).ToList();
            ViewBag.MonthlyWaste   = monthly.Select(m => m.Waste).ToList();
            ViewBag.MonthlyRate    = monthly.Select(m => m.Consumed > 0 ? Math.Round(m.Waste / m.Consumed * 100, 2) : 0).ToList();
            ViewBag.Machines   = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.Products   = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.StartDate  = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate    = endDate?.ToString("yyyy-MM-dd");
            ViewBag.MachineId  = machineId;
            ViewBag.ProductId  = productId;
            ViewData["Title"]  = "Fire Raporu";
            return View(details);
        }

        // GET /Production/ExportWasteReport
        public async Task<IActionResult> ExportWasteReport(DateTime? startDate, DateTime? endDate, int? machineId, int? productId)
        {
            var query = _context.ProductionDetails
                .Include(d => d.WorkOrder).ThenInclude(w => w.Product)
                .Include(d => d.FscSerial).ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(d => d.Machine)
                .Where(d => d.WasteWeight > 0)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(d => d.ProductionDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(d => d.ProductionDate <= endDate.Value.AddDays(1));
            if (machineId.HasValue) query = query.Where(d => d.MachineId == machineId.Value);
            if (productId.HasValue) query = query.Where(d => d.WorkOrder.ProductId == productId.Value);

            var data = await query.OrderByDescending(d => d.ProductionDate).ToListAsync();
            var rows = data.Select(d => {
                var fr = d.ConsumedWeight > 0 ? Math.Round(d.WasteWeight / d.ConsumedWeight * 100, 2) : 0;
                return new {
                    Tarih        = d.ProductionDate.ToString("dd.MM.yyyy"),
                    IsEmriNo     = d.WorkOrder?.WorkOrderNo,
                    Urun         = d.WorkOrder?.Product?.ProductName,
                    SerialNo     = d.FscSerial?.SerialNo,
                    LotNo        = d.FscSerial?.Lot?.LotNo,
                    Tedarikci    = d.FscSerial?.Lot?.Supplier?.Name,
                    Makine       = d.Machine?.Name,
                    TuketimKg    = d.ConsumedWeight,
                    FireKg       = d.WasteWeight,
                    FireOranPct  = fr,
                    Notlar       = d.Notes
                };
            });
            return ExportToExcel(rows, "FireRaporu");
        }

        // ─── WasteManagement (İmha Kayıtları) ───────────────────────────────

        // GET /Production/WasteManagement
        public async Task<IActionResult> WasteManagement()
        {
            ViewData["Title"] = "İmha / Atık Kayıtları";
            var list = await _context.WasteManagements
                .Include(w => w.WorkOrder).ThenInclude(wo => wo!.Product)
                .OrderByDescending(w => w.DisposalDate)
                .ToListAsync();

            ViewBag.WorkOrders = await _context.WorkOrders
                .Include(wo => wo.Product)
                .OrderByDescending(wo => wo.Id)
                .Take(50)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> GetWaste(int id)
        {
            var item = await _context.WasteManagements.FindAsync(id);
            if (item == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                item.Id, item.WasteCode, item.WorkOrderId, item.Category,
                item.Description, item.Quantity, item.Unit,
                disposalDate = item.DisposalDate.ToString("yyyy-MM-dd"),
                item.DisposalMethod, item.DisposedBy, item.Notes
            }});
        }

        [HttpPost]
        public async Task<IActionResult> SaveWaste(FSCTakip.Core.Entities.WasteManagement model)
        {
            try
            {
                if (model.Id == 0)
                {
                    if (string.IsNullOrWhiteSpace(model.WasteCode))
                    {
                        var count = await _context.WasteManagements.CountAsync(w => w.CreatedDate.Year == DateTime.Now.Year);
                        model.WasteCode = $"ATK{DateTime.Now.Year}-{count + 1:D3}";
                    }
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy   = User.Identity?.Name ?? "System";
                    _context.WasteManagements.Add(model);
                }
                else
                {
                    var existing = await _context.WasteManagements.FindAsync(model.Id);
                    if (existing == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                    existing.WorkOrderId    = model.WorkOrderId;
                    existing.Category       = model.Category;
                    existing.Description    = model.Description;
                    existing.Quantity       = model.Quantity;
                    existing.Unit           = model.Unit;
                    existing.DisposalDate   = model.DisposalDate;
                    existing.DisposalMethod = model.DisposalMethod;
                    existing.DisposedBy     = model.DisposedBy;
                    existing.Notes          = model.Notes;
                    existing.UpdatedDate    = DateTime.Now;
                    existing.UpdatedBy      = User.Identity?.Name ?? "System";
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kayıt kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWaste(int id)
        {
            var item = await _context.WasteManagements.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            _context.WasteManagements.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}

public class WasteGroupRow
{
    public string Label         { get; set; } = "";
    public int    RecordCount   { get; set; }
    public decimal TotalConsumed { get; set; }
    public decimal TotalWaste   { get; set; }
    public decimal WasteRate    => TotalConsumed > 0 ? Math.Round(TotalWaste / TotalConsumed * 100, 2) : 0;
}
