using ClosedXML.Excel;
using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace FSCTakip.WebUI.Controllers
{
    public class SalesController : BaseController
    {
        public SalesController(AppDbContext context) : base(context) { }

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

            ViewBag.Products   = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.WorkOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Where(w => w.Status == WorkOrderStatus.Tamamlandi)
                .OrderByDescending(w => w.Id)
                .ToListAsync();

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
            SalesOrderStatus status, string? notes)
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
                    Currency        = currency,
                    PlateNumber     = plateNumber,
                    DeliveryAddress = deliveryAddress,
                    Status          = status,
                    Notes           = notes
                };
                _context.SalesOrders.Add(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Sipariş oluşturuldu", salesOrderNo = order.SalesOrderNo, id = order.Id });
            }
            else
            {
                var order = await _context.SalesOrders.FindAsync(salesOrderId);
                if (order == null) return Json(new { success = false, message = "Kayıt bulunamadı" });

                order.CustomerId      = customerId;
                order.OrderDate       = orderDate;
                order.ExternalOrderNo = string.IsNullOrWhiteSpace(externalOrderNo) ? order.ExternalOrderNo : externalOrderNo.Trim();
                order.DispatchNo      = dispatchNo;
                order.InvoiceNo       = invoiceNo;
                order.InvoiceAmount   = invoiceAmount;
                order.Currency        = currency;
                order.PlateNumber     = plateNumber;
                order.DeliveryAddress = deliveryAddress;
                order.Status          = status;
                order.Notes           = notes;
                order.UpdatedDate     = DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Sipariş güncellendi", salesOrderNo = order.SalesOrderNo, id = order.Id });
            }
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
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });
            if (order.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Teslim edilmiş siparişe kalem eklenemez" });

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
                if (line == null) return Json(new { success = false, message = "Kalem bulunamadı" });
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
            if (line == null) return Json(new { success = false, message = "Kalem bulunamadı" });
            if (line.SalesOrder.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Teslim edilmiş siparişten kalem silinemez" });

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
            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });
            if (order.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Teslim edilmiş sipariş silinemez" });

            try
            {
                _context.SalesOrders.Remove(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Sipariş silindi" });
            }
            catch (FSCTakip.Core.Entities.PeriodLockedException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Bu sipariş silinemez. {ex.Message}" });
            }
        }

        // POST /Sales/Dispatch/{id}  — Sevk Et: durum → TeslimEdildi + StockMovement
        [HttpPost]
        public async Task<IActionResult> Dispatch(int id, DateTime? dispatchDate, string? dispatchNo, string? plateNumber)
        {
            var order = await _context.SalesOrders
                .Include(s => s.Lines).ThenInclude(l => l.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (order == null) return Json(new { success = false, message = "Sipariş bulunamadı" });
            if (order.Status == SalesOrderStatus.TeslimEdildi)
                return Json(new { success = false, message = "Sipariş zaten teslim edildi" });
            if (!order.Lines.Any())
                return Json(new { success = false, message = "Siparişte kalem yok, sevk edilemez" });

            var actualDate = dispatchDate ?? DateTime.Today;
            order.DispatchDate = actualDate;
            if (!string.IsNullOrWhiteSpace(dispatchNo))  order.DispatchNo  = dispatchNo;
            if (!string.IsNullOrWhiteSpace(plateNumber)) order.PlateNumber = plateNumber;
            order.Status = SalesOrderStatus.TeslimEdildi;
            order.UpdatedDate = DateTime.Now;

            // Her kalem için StockMovement oluştur
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
                    Description  = $"Sevkiyat: {order.Customer.Name}"
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
                                    : s.Status == SalesOrderStatus.TeslimEdildi ? "Teslim Edildi" : "İptal"
                })
                .ToListAsync();

            return ExportToExcel(rows, "SatisListesi");
        }
    }
}
