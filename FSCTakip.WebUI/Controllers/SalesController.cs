using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace FSCTakip.WebUI.Controllers
{
    public class SalesController : BaseController
    {
        private readonly IFileStorageService _storage;

        public SalesController(AppDbContext context, IFileStorageService storage) : base(context)
        {
            _storage = storage;
        }

        // POST /Sales/UploadDocument â€” satÄ±ÅŸ irsaliye/fatura belgesi yÃ¼kle
        [HttpPost]
        public async Task<IActionResult> UploadDocument(int orderId, string docType, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Dosya seÃ§ilmedi." });

                var order = await _context.SalesOrders.FindAsync(orderId);
                if (order == null) return Json(new { success = false, message = "SipariÅŸ bulunamadÄ±." });

                var path = await _storage.SaveAsync(file, docType == "invoice" ? "Invoice" : "Dispatch");

                if (docType == "invoice")
                    order.InvoicePdfPath = path;
                else
                    order.DispatchPdfPath = path;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Belge yÃ¼klendi.", path });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET /Sales/Index
        public async Task<IActionResult> Index(
            DateTime? startDate, DateTime? endDate,
            int? customerId, SalesOrderStatus? status)
        {
            var query = _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(s => s.OrderDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(s => s.OrderDate <= endDate.Value.AddDays(1));
            if (customerId.HasValue) query = query.Where(s => s.CustomerId == customerId.Value);
            if (status.HasValue)    query = query.Where(s => s.Status == status.Value);

            ViewBag.Customers  = await _context.Customers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            ViewBag.StartDate  = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate    = endDate?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(s => s.Id).ToListAsync());
        }

        // GET /Sales/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Product)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.WorkOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (order == null) return NotFound();

            ViewBag.Products   = await _context.Products.Include(p => p.ProductGroup).Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            var workOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Where(w => w.Status == WorkOrderStatus.Tamamlandi)
                .OrderByDescending(w => w.Id)
                .ToListAsync();

            var dispatchedByWo = await _context.StockMovements
                .Where(sm => sm.Type == MovementType.SalesDispatch && sm.WorkOrderId != null)
                .GroupBy(sm => sm.WorkOrderId!.Value)
                .Select(g => new { WorkOrderId = g.Key, Total = g.Sum(sm => sm.Quantity) })
                .ToDictionaryAsync(x => x.WorkOrderId, x => x.Total);

            ViewBag.WorkOrders = workOrders;
            ViewBag.WorkOrderRemaining = workOrders.ToDictionary(
                w => w.Id,
                w => w.ActualQuantity - (dispatchedByWo.TryGetValue(w.Id, out var d) ? d : 0m));

            return View(order);
        }

        // GET /Sales/Print/{id} -- FSC beyanli sevk belgesi
        public async Task<IActionResult> Print(int id)
        {
            var order = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Product).ThenInclude(p => p!.FscType)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.WorkOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // GET /Sales/PrintInvoice/{id} -- satis faturasi
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var order = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Product).ThenInclude(p => p!.FscType)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.WorkOrder)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // GET /Sales/GetOrder/{id}
        [HttpGet]
        public async Task<IActionResult> GetOrder(int id)
        {
            var s = await _context.SalesOrders.FindAsync(id);
            if (s == null) return Json(new { success = false });
            return Json(new
            {
                success = true,
                data = new
                {
                    s.Id, s.SalesOrderNo, s.CustomerId,
                    orderDate       = s.OrderDate.ToString("yyyy-MM-dd"),
                    dispatchDate    = s.DispatchDate?.ToString("yyyy-MM-dd"),
                    invoiceDate     = s.InvoiceDate?.ToString("yyyy-MM-dd"),
                    s.DispatchNo, s.InvoiceNo,
                    s.InvoiceAmount, s.Currency,
                    s.PlateNumber, s.DeliveryAddress,
                    status          = (int)s.Status, s.Notes,
                    s.ExternalOrderNo
                }
            });
        }

        // POST /Sales/SaveOrder
        [HttpPost]
        public async Task<IActionResult> SaveOrder(
            int salesOrderId, int customerId,
            DateTime orderDate, string? externalOrderNo, string? dispatchNo, string? invoiceNo,
            decimal? invoiceAmount, string currency,
            string? plateNumber, string? deliveryAddress,
            SalesOrderStatus status, string? notes, DateTime? invoiceDate)
        {
            try
            {
                if (salesOrderId == 0)
                {
                    var count = await _context.SalesOrders.CountAsync();
                    var order = new SalesOrder
                    {
                        SalesOrderNo    = $"SIP{DateTime.Today.Year}-{count + 1:D3}",
                        ExternalOrderNo = string.IsNullOrWhiteSpace(externalOrderNo) ? null : externalOrderNo.Trim(),
                        CustomerId      = customerId,
                        OrderDate       = orderDate,
                        DispatchNo      = dispatchNo,
                        InvoiceNo       = invoiceNo,
                        InvoiceAmount   = invoiceAmount,
                        InvoiceDate     = invoiceDate,
                        Currency        = currency,
                        PlateNumber     = plateNumber,
                        DeliveryAddress = deliveryAddress,
                        Status          = status,
                        Notes           = notes
                    };
                    _context.SalesOrders.Add(order);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "SipariÅŸ oluÅŸturuldu", salesOrderNo = order.SalesOrderNo, id = order.Id });
                }
                else
                {
                    var order = await _context.SalesOrders.FindAsync(salesOrderId);
                    if (order == null) return Json(new { success = false, message = "KayÄ±t bulunamadÄ±" });

                    order.CustomerId      = customerId;
                    order.OrderDate       = orderDate;
                    order.ExternalOrderNo = string.IsNullOrWhiteSpace(externalOrderNo) ? order.ExternalOrderNo : externalOrderNo.Trim();
                    order.DispatchNo      = dispatchNo;
                    order.InvoiceNo       = invoiceNo;
                    order.InvoiceAmount   = invoiceAmount;
                    order.InvoiceDate     = invoiceDate;
                    order.Currency        = currency;
                    order.PlateNumber     = plateNumber;
                    order.DeliveryAddress = deliveryAddress;
                    order.Status          = status;
                    order.Notes           = notes;
                    order.UpdatedDate     = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "SipariÅŸ gÃ¼ncellendi", salesOrderNo = order.SalesOrderNo, id = order.Id });
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // GET /Sales/GetLine/{id}
        [HttpGet]
        public async Task<IActionResult> GetLine(int id)
        {
            var l = await _context.SalesOrderLines.FindAsync(id);
            if (l == null) return Json(new { success = false });
            return Json(new
            {
                success = true,
                data = new
                {
                    l.Id, l.ProductId, l.WorkOrderId,
                    l.Quantity, l.UnitPrice, l.Unit, l.Notes
                }
            });
        }

        // POST /Sales/SaveLine
        [HttpPost]
        public async Task<IActionResult> SaveLine(
            int lineId, int salesOrderId,
            int productId, int? workOrderId,
            decimal quantity, decimal unitPrice,
            string unit, string? notes)
        {
            var order = await _context.SalesOrders.FindAsync(salesOrderId);
            if (order == null) return Json(new { success = false, message = "SipariÅŸ bulunamadÄ±" });
            if (order.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Teslim edilmiÅŸ sipariÅŸe kalem eklenemez" });

            if (lineId == 0)
            {
                var line = new SalesOrderLine
                {
                    SalesOrderId = salesOrderId,
                    ProductId    = productId,
                    WorkOrderId  = workOrderId == 0 ? null : workOrderId,
                    Quantity     = quantity,
                    UnitPrice    = unitPrice,
                    Unit         = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit,
                    Notes        = notes
                };
                _context.SalesOrderLines.Add(line);
            }
            else
            {
                var line = await _context.SalesOrderLines.FindAsync(lineId);
                if (line == null) return Json(new { success = false, message = "Kalem bulunamadÄ±" });
                line.ProductId   = productId;
                line.WorkOrderId = workOrderId == 0 ? null : workOrderId;
                line.Quantity    = quantity;
                line.UnitPrice   = unitPrice;
                line.Unit        = string.IsNullOrWhiteSpace(unit) ? "Adet" : unit;
                line.Notes       = notes;
                line.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Kalem kaydedildi" });
        }

        // POST /Sales/DeleteLine/{id}
        [HttpPost]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var line = await _context.SalesOrderLines
                .Include(l => l.SalesOrder)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (line == null) return Json(new { success = false, message = "Kalem bulunamadÄ±" });
            if (line.SalesOrder.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Teslim edilmiÅŸ sipariÅŸten kalem silinemez" });

            try
            {
                _context.SalesOrderLines.Remove(line);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Kalem silindi" });
            }
            catch (FSCTakip.Core.Entities.PeriodLockedException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Bu kalem silinemez. {ex.Message}" });
            }
        }

        // POST /Sales/DeleteOrder/{id}
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.SalesOrders.Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == id);
            if (order == null) return Json(new { success = false, message = "SipariÅŸ bulunamadÄ±" });
            if (order.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Teslim edilmiÅŸ sipariÅŸ silinemez" });

            try
            {
                _context.SalesOrders.Remove(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "SipariÅŸ silindi" });
            }
            catch (FSCTakip.Core.Entities.PeriodLockedException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Bu sipariÅŸ silinemez. {ex.Message}" });
            }
        }

        // POST /Sales/Dispatch/{id}  â€” Sevk Et: durum â†’ TeslimEdildi + StockMovement
        [HttpPost]
        public async Task<IActionResult> Dispatch(int id, DateTime? dispatchDate, string? dispatchNo, string? plateNumber)
        {
            var order = await _context.SalesOrders
                .Include(s => s.Lines).ThenInclude(l => l.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (order == null) return Json(new { success = false, message = "SipariÅŸ bulunamadÄ±" });
            if (order.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "SipariÅŸ zaten teslim edildi" });
            if (order.Customer?.IsFscActive == false ||
                (order.Customer?.FscExpiryDate.HasValue == true && order.Customer.FscExpiryDate.Value < DateTime.Today))
                return Json(new { success = false, message = $"{order.Customer?.Name} firmasÄ±nÄ±n FSC sertifikasÄ± geÃ§ersiz veya sÃ¼resi dolmuÅŸ. Sevkiyat engellenmiÅŸtir." });
            if (!order.Lines.Any())
                return Json(new { success = false, message = "SipariÅŸte kalem yok, sevk edilemez" });

            var actualDate = dispatchDate ?? DateTime.Today;

            // Stok yeterlilik kontrolu: her WorkOrder icin uretilen miktar asilamaz (negatif bakiye engeli)
            var woGroups = order.Lines.Where(l => l.WorkOrderId.HasValue)
                                       .GroupBy(l => l.WorkOrderId!.Value);
            foreach (var grp in woGroups)
            {
                var workOrder = await _context.WorkOrders.FindAsync(grp.Key);
                if (workOrder == null) continue;

                var alreadyDispatched = await _context.StockMovements
                    .Where(sm => sm.Type == MovementType.SalesDispatch && sm.WorkOrderId == grp.Key)
                    .SumAsync(sm => sm.Quantity);
                var requestedNow = grp.Sum(l => l.Quantity);
                var remaining = workOrder.ActualQuantity - alreadyDispatched;

                if (requestedNow > remaining)
                    return Json(new { success = false, message = $"{workOrder.WorkOrderNo}: Kalan sevk edilebilir miktar {remaining:N0} adet, istenen {requestedNow:N0} adet. Sevkiyat engellendi (stok yetersiz)." });
            }

            order.DispatchDate = actualDate;
            if (!string.IsNullOrWhiteSpace(dispatchNo))  order.DispatchNo  = dispatchNo;
            if (!string.IsNullOrWhiteSpace(plateNumber)) order.PlateNumber = plateNumber;
            order.Status = SalesOrderStatus.TeslimEdildi;
            order.UpdatedDate = DateTime.Now;

            // Her kalem iÃ§in StockMovement oluÅŸtur
            foreach (var line in order.Lines)
            {
                _context.StockMovements.Add(new StockMovement
                {
                    Type         = MovementType.SalesDispatch,
                    DocumentNo   = order.SalesOrderNo,
                    DocumentDate = actualDate,
                    ProductId    = line.ProductId,
                    Quantity     = line.Quantity,
                    Unit         = line.Unit,
                    CustomerId   = order.CustomerId,
                    PlateNumber  = order.PlateNumber,
                    WorkOrderId  = line.WorkOrderId,
                    Description  = $"Sevkiyat: {order.Customer.Name}",
                    CreatedBy    = User.Identity?.Name ?? "System",
                    CreatedDate  = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{order.SalesOrderNo} sevk edildi" });
        }

        // GET /Sales/ExportIndex
        public async Task<IActionResult> ExportIndex()
        {
            var rows = await _context.SalesOrders
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .OrderByDescending(s => s.Id)
                .Select(s => new
                {
                    SiparisNo     = s.SalesOrderNo,
                    Musteri       = s.Customer != null ? s.Customer.Name : "",
                    SiparisTarihi = s.OrderDate.ToString("dd.MM.yyyy"),
                    SevkTarihi    = s.DispatchDate != null ? s.DispatchDate.Value.ToString("dd.MM.yyyy") : "",
                    IrsaliyeNo    = s.DispatchNo ?? "",
                    FaturaNo      = s.InvoiceNo ?? "",
                    Tutar         = s.InvoiceAmount != null ? s.InvoiceAmount.Value.ToString("N2") : "",
                    ParaBirimi    = s.Currency,
                    KalemSayisi   = s.Lines.Count(),
                    ToplamAdet    = s.Lines.Sum(l => l.Quantity),
                    Durum         = s.Status == SalesOrderStatus.Taslak ? "Taslak"
                                    : s.Status == SalesOrderStatus.TeslimEdildi ? "Teslim Edildi" : "Ä°ptal"
                })
                .ToListAsync();

            return ExportToExcel(rows, "SatisListesi");
        }
    }
}

