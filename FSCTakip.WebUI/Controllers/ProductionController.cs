using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class ProductionController : BaseController
    {
        public ProductionController(AppDbContext context) : base(context) { }

        // GET /Production/Index
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int[]? productIds, WorkOrderStatus? status)
        {
            var query = _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.Machine)
                .Include(w => w.ProductionDetails)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(w => w.PlannedDate >= startDate.Value);
            if (endDate.HasValue)   query = query.Where(w => w.PlannedDate <= endDate.Value.AddDays(1));
            if (productIds != null && productIds.Length > 0)
                query = query.Where(w => productIds.Contains(w.ProductId));
            if (status.HasValue)    query = query.Where(w => w.Status == status.Value);

            ViewBag.Products  = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            ViewBag.Machines  = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate   = endDate?.ToString("yyyy-MM-dd");
            ViewBag.ProductIds = productIds ?? Array.Empty<int>();

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
                w.PlannedQuantity, w.Notes, status = (int)w.Status,
                w.ExternalOrderNo
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
                {
                    model.ExternalOrderNo = string.IsNullOrWhiteSpace(model.ExternalOrderNo) ? null : model.ExternalOrderNo.Trim();
                    _context.WorkOrders.Add(model);
                }
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
                    existing.ExternalOrderNo = string.IsNullOrWhiteSpace(model.ExternalOrderNo) ? existing.ExternalOrderNo : model.ExternalOrderNo.Trim();
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "İş emri kaydedildi.", workOrderNo = model.WorkOrderNo, workOrderId = model.Id });
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

                // FSC sertifikalı BOM bileşenlerinde tüketim zorunlu (CoC zinciri)
                var fscExempt = await _context.WorkOrderRecipes
                    .Include(r => r.Product).ThenInclude(p => p!.FscType)
                    .Where(r => r.WorkOrderId == id && r.Product.FscTypeId != null && r.ActualConsumedQuantity == 0)
                    .Select(r => r.Product.ProductName)
                    .ToListAsync();

                if (fscExempt.Any())
                    return Json(new {
                        success = false,
                        message = "FSC sertifikalı bileşenler için tüketim kaydı eksik:\n• " +
                                  string.Join("\n• ", fscExempt) +
                                  "\n\nFSC CoC zinciri tamamlanmadan iş emri kapatılamaz."
                    });

                wo.Status        = WorkOrderStatus.Tamamlandi;
                wo.CompletedDate = DateTime.Now;
                // Üretilen adet: her malzeme satırı (hangi güne ait olursa olsun) iş emrinin
                // TOPLAM kümülatif üretimini taşır — fiş 1 günde de kapansa 1 haftada da tektir,
                // günlere göre toplanmaz. Tüm satırlar arasından tek MAX alınır.
                var prodDetails = await _context.ProductionDetails
                    .Where(d => d.WorkOrderId == id)
                    .ToListAsync();
                wo.ActualQuantity = prodDetails.Any()
                    ? prodDetails.Max(d => d.ProducedQuantity)
                    : 0;

                // Mamul → bitmiş ürün stoğuna giriş (ProductionEntry). İş emriyle eşlenir; yeniden tamamlamada güncellenir.
                var mamulEntry = await _context.StockMovements.FirstOrDefaultAsync(
                    m => m.Type == MovementType.ProductionEntry && m.WorkOrderId == wo.Id && m.ProductId == wo.ProductId);
                if (wo.ActualQuantity > 0)
                {
                    if (mamulEntry == null)
                    {
                        _context.StockMovements.Add(new StockMovement
                        {
                            Type         = MovementType.ProductionEntry,
                            ProductId    = wo.ProductId,
                            Quantity     = wo.ActualQuantity,
                            Unit         = "adet",
                            DocumentNo   = wo.WorkOrderNo,
                            DocumentDate = wo.CompletedDate ?? DateTime.Now,
                            WorkOrderId  = wo.Id,
                            Description  = $"Üretimden giriş — {wo.WorkOrderNo}",
                            CreatedBy    = User.Identity?.Name ?? "System",
                            CreatedDate  = DateTime.Now
                        });
                    }
                    else
                    {
                        mamulEntry.Quantity     = wo.ActualQuantity;
                        mamulEntry.DocumentDate = wo.CompletedDate ?? DateTime.Now;
                    }
                }
                else if (mamulEntry != null)
                {
                    _context.StockMovements.Remove(mamulEntry);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "İş emri tamamlandı olarak işaretlendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/RecalcAllActualQty — Mevcut yanlış ActualQuantity değerlerini düzelt (admin)
        [HttpPost]
        public async Task<IActionResult> RecalcAllActualQty()
        {
            if (!IsAdminUser)
                return Json(new { success = false, message = "Bu işlem yalnızca admin tarafından yapılabilir." });

            var workOrders = await _context.WorkOrders
                .Where(w => w.Status != WorkOrderStatus.Taslak)
                .ToListAsync();

            var allDetails = await _context.ProductionDetails.ToListAsync();
            var detailsByWo = allDetails.GroupBy(d => d.WorkOrderId)
                              .ToDictionary(g => g.Key, g => g.ToList());

            var prodEntryMovements = await _context.StockMovements
                .Where(sm => sm.Type == MovementType.ProductionEntry && sm.WorkOrderId != null)
                .ToListAsync();
            var movementsByWo = prodEntryMovements
                .Where(sm => sm.WorkOrderId.HasValue)
                .GroupBy(sm => sm.WorkOrderId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            int updated = 0;
            int movementsFixed = 0;
            foreach (var wo in workOrders)
            {
                if (!detailsByWo.TryGetValue(wo.Id, out var details) || !details.Any()) continue;
                // Fiş 1 günde de kapansa 1 haftada da tektir — üretim adedi kümülatif, günlere göre toplanmaz.
                var correct = details.Max(d => d.ProducedQuantity);
                if (wo.ActualQuantity != correct)
                {
                    wo.ActualQuantity = correct;
                    updated++;
                }

                // İlgili "Üretimden Giriş" stok hareketini de aynı doğru değere senkronize et.
                if (movementsByWo.TryGetValue(wo.Id, out var mov))
                {
                    if (mov.Quantity != correct)
                    {
                        mov.Quantity = correct;
                        movementsFixed++;
                    }
                }
                else if (correct > 0)
                {
                    _context.StockMovements.Add(new StockMovement
                    {
                        Type         = MovementType.ProductionEntry,
                        ProductId    = wo.ProductId,
                        Quantity     = correct,
                        Unit         = "adet",
                        DocumentNo   = wo.WorkOrderNo,
                        DocumentDate = wo.CompletedDate ?? DateTime.Now,
                        WorkOrderId  = wo.Id,
                        Description  = $"Üretimden giriş — {wo.WorkOrderNo}",
                        CreatedBy    = User.Identity?.Name ?? "System",
                        CreatedDate  = DateTime.Now
                    });
                    movementsFixed++;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{updated} iş emrinin üretim adedi, {movementsFixed} stok hareketi güncellendi." });
        }

        // POST /Production/RecalcCurrentWeightFire — eski kayitlardaki fire dusulmemis CurrentWeight duzeltme (one-time admin)
        [HttpPost]
        public async Task<IActionResult> RecalcCurrentWeightFire()
        {
            // Uretimde fire olan tum serillerin CurrentWeight = InitialWeight - SUM(consumed + fire)
            var details = await _context.ProductionDetails
                .GroupBy(d => d.FscSerialId)
                .Select(g => new {
                    SerialId      = g.Key,
                    TotalConsumed = g.Sum(d => d.ConsumedWeight),
                    TotalFire     = g.Sum(d => d.WasteWeight)
                })
                .ToListAsync();

            var serialIds = details.Select(d => d.SerialId).ToList();
            var serials = await _context.FscSerials
                .Where(s => serialIds.Contains(s.Id))
                .ToListAsync();

            int updated = 0;
            foreach (var s in serials)
            {
                var det = details.First(d => d.SerialId == s.Id);
                var correct = s.InitialWeight - det.TotalConsumed - det.TotalFire;
                if (correct < 0) correct = 0;
                if (s.CurrentWeight != correct)
                {
                    s.CurrentWeight = correct;
                    updated++;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{updated} bobinin stok bakiyesi fire dusulerek duzeltildi." });
        }


        // POST /Production/RecalcStockMovementsFire -- eski StockMovement Quantity = consumed only, guncelle consumed+fire
        [HttpPost]
        public async Task<IActionResult> RecalcStockMovementsFire()
        {
            var details = await _context.ProductionDetails
                .Select(d => new { d.Id, d.ConsumedWeight, d.WasteWeight, d.FscSerialId, d.WorkOrderId, d.ProductionDate })
                .ToListAsync();

            if (!details.Any())
                return Json(new { success = true, message = "Islenecek uretim kaydi yok." });

            var detailIds = details.Select(d => d.Id).ToList();
            var movements = await _context.StockMovements
                .Where(m => m.Type == MovementType.ProductionConsumption
                         && m.ErpReferenceId != null
                         && detailIds.Contains(m.ErpReferenceId.Value))
                .ToListAsync();
            var movMap = movements.ToDictionary(m => m.ErpReferenceId!.Value);

            var serialIds = details.Select(d => d.FscSerialId).Distinct().ToList();
            var serialProductMap = await _context.FscSerials
                .Where(s => serialIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Lot.ProductId })
                .ToDictionaryAsync(x => x.Id, x => (int?)x.ProductId);

            var woIds = details.Select(d => d.WorkOrderId).Distinct().ToList();
            var woNoMap = await _context.WorkOrders
                .Where(w => woIds.Contains(w.Id))
                .Select(w => new { w.Id, w.WorkOrderNo })
                .ToDictionaryAsync(w => w.Id, w => w.WorkOrderNo);

            int updated = 0, created = 0;
            string user = User.Identity?.Name ?? "System";

            foreach (var d in details)
            {
                var correctQty = d.ConsumedWeight + d.WasteWeight;
                if (movMap.TryGetValue(d.Id, out var sm))
                {
                    if (sm.Quantity != correctQty)
                    {
                        sm.Quantity = correctQty;
                        updated++;
                    }
                }
                else
                {
                    if (!serialProductMap.TryGetValue(d.FscSerialId, out var prodId) || prodId == null)
                        continue;
                    var woNo = woNoMap.TryGetValue(d.WorkOrderId, out var no) ? no : "";
                    _context.StockMovements.Add(new StockMovement
                    {
                        Type           = MovementType.ProductionConsumption,
                        ErpReferenceId = d.Id,
                        ProductId      = prodId.Value,
                        Quantity       = correctQty,
                        Unit           = "kg",
                        DocumentNo     = woNo,
                        DocumentDate   = d.ProductionDate,
                        WorkOrderId    = d.WorkOrderId,
                        Description    = "Uretim tuketimi (recalc)",
                        CreatedBy      = user,
                        CreatedDate    = DateTime.Now
                    });
                    created++;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = updated + " stok hareketi guncellendi, " + created + " eksik hareket olusturuldu." });
        }
        // GET /Production/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var wo = await _context.WorkOrders
                .Include(w => w.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.Machine)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.FscSerial)
                        .ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.FscSerial)
                        .ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.Machine)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.WorkOrderRecipe)
                        .ThenInclude(r => r!.Product)
                .Include(w => w.WorkOrderRecipes)
                    .ThenInclude(r => r.Product).ThenInclude(p => p.FscType)
                .Include(w => w.WorkOrderRecipes)
                    .ThenInclude(r => r.FscSerial).ThenInclude(s => s!.Lot)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wo == null) return NotFound();

            // Hammadde bobinleri (kalan > 0 olanlar)
            var availableSerials = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                // Dönüşümden gelen YM'ler için kaynak hammadde izlenebilirliği
                .Include(s => s.Lot).ThenInclude(l => l.SourceSerial)
                    .ThenInclude(src => src!.Lot).ThenInclude(sl => sl.Product)
                .Include(s => s.Lot).ThenInclude(l => l.SourceSerial)
                    .ThenInclude(src => src!.Lot).ThenInclude(sl => sl.Supplier)
                .Where(s => s.CurrentWeight > 0)
                .OrderBy(s => s.Lot.PartiNo).ThenBy(s => s.SerialNo)
                .AsSplitQuery()
                .ToListAsync();

            // Ürünün tanımlı reçete bileşenleri (BOM dropdown için)
            var recipeComponents = await _context.ProductRecipes
                .Include(r => r.ChildProduct).ThenInclude(p => p.FscType)
                .Where(r => r.ParentProductId == wo.ProductId && r.IsActive)
                .OrderBy(r => r.ChildProduct.ProductName)
                .ToListAsync();

            // WorkOrderRecipes — mevcut reçete satırları (varsa)
            var workOrderRecipes = wo.WorkOrderRecipes.OrderBy(r => r.Product?.ProductName).ToList();

            ViewBag.AvailableSerials   = availableSerials;
            ViewBag.Machines           = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.RecipeComponents   = recipeComponents;
            ViewBag.WorkOrderRecipes   = workOrderRecipes;
            ViewBag.IsAdmin            = IsAdminUser;
            ViewData["Title"] = $"İş Emri — {wo.WorkOrderNo}";
            return View(wo);
        }

        // GET /Production/PrintForm?ids=1&ids=2
        [HttpGet]
        public async Task<IActionResult> PrintForm(int[] ids)
        {
            if (ids == null || ids.Length == 0) return NotFound();

            var workOrders = await _context.WorkOrders
                .Include(w => w.Product).ThenInclude(p => p!.FscType)
                .Include(w => w.Machine)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.FscSerial)
                        .ThenInclude(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.FscSerial)
                        .ThenInclude(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(w => w.ProductionDetails)
                    .ThenInclude(d => d.FscSerial)
                        .ThenInclude(s => s.Lot).ThenInclude(l => l.Product)
                .Where(w => ids.Contains(w.Id))
                .AsSplitQuery()
                .ToListAsync();

            var ordered = ids.Distinct()
                .Select(id => workOrders.FirstOrDefault(w => w.Id == id))
                .Where(w => w != null)
                .Select(w => w!)
                .ToList();

            if (ordered.Count == 0) return NotFound();

            return View(ordered);
        }

        // GET /Production/GetLotDocuments/{serialId}
        [HttpGet]
        public async Task<IActionResult> GetLotDocuments(int serialId)
        {
            var serial = await _context.FscSerials
                .Include(s => s.Lot).ThenInclude(l => l.Supplier)
                .Include(s => s.Lot).ThenInclude(l => l.FscType)
                .Include(s => s.Lot).ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(s => s.Id == serialId);

            if (serial == null) return Json(new { success = false, message = "Bobin bulunamadı." });

            var lot = serial.Lot;
            return Json(new { success = true, data = new {
                lotId           = lot.Id,
                lotNo           = lot.PartiNo,
                serialNo        = serial.SerialNo,
                supplier        = lot.Supplier?.Name,
                supplierCode    = lot.Supplier?.SupplierCode,
                fscType         = lot.FscType?.Name,
                fscCode         = lot.FscType?.Code,
                product         = lot.Product?.ProductName,
                productCode     = lot.Product?.ProductCode,
                arrivalDate     = lot.ArrivalDate.ToString("dd.MM.yyyy"),
                invoiceNo       = lot.InvoiceNo,
                dispatchNo      = lot.DispatchNo,
                truckPlate      = lot.TruckPlate,
                invoicePdfPath  = lot.InvoicePdfPath,
                dispatchPdfPath = lot.DispatchPdfPath,
                invoiceAmount   = lot.InvoiceAmount,
                currency        = lot.Currency ?? "TRY"
            }});
        }

        // GET /Production/GetDetail/{id}
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var d = await _context.ProductionDetails.FindAsync(id);
            if (d == null) return Json(new { success = false });
            return Json(new { success = true, data = new {
                d.Id, d.WorkOrderId, d.FscSerialId, d.MachineId,
                workOrderRecipeId = d.WorkOrderRecipeId,
                productionDate = d.ProductionDate.ToString("yyyy-MM-dd"),
                d.ConsumedWeight, d.WasteWeight, d.ProducedQuantity, d.Notes
            }});
        }

        // POST /Production/SaveDetail
        [HttpPost]
        public async Task<IActionResult> SaveDetail(ProductionDetail model, string? correctionReason = null)
        {
            try
            {
                if (model.ConsumedWeight <= 0)
                    return Json(new { success = false, message = "Tüketim miktarı sıfırdan büyük olmalıdır." });
                if (model.WasteWeight < 0)
                    return Json(new { success = false, message = "Fire miktarı negatif olamaz." });
                if (model.ProducedQuantity <= 0)
                    return Json(new { success = false, message = "Üretilen adet sıfırdan büyük olmalıdır." });

                // Mevcut kayıt düzeltmesi: yalnızca admin, yalnızca neden belirtilmişse
                if (model.Id > 0)
                {
                    if (!IsAdminUser)
                        return Json(new { success = false, message = "Mevcut tüketim kayıtlarını yalnızca admin düzeltebilir." });
                    if (string.IsNullOrWhiteSpace(correctionReason))
                        return Json(new { success = false, message = "Düzeltme nedeni zorunludur." });
                }
                // Fire, tuketimden bagimsiz -- ayri kayip kalemi, toplam dusus = consumed + fire


                // Tamamlanmis is emrine tuketim kaydedilemez
                var wo = await _context.WorkOrders.FindAsync(model.WorkOrderId);
                if (wo == null)
                    return Json(new { success = false, message = "Is emri bulunamadi." });
                if (wo.Status == WorkOrderStatus.Tamamlandi)
                    return Json(new { success = false, message = "Tamamlanmis is emrine tuketim kaydedilemez." });

                var serial = await _context.FscSerials
                    .Include(s => s.ProductionDetails)
                    .Include(s => s.Lot)
                    .FirstOrDefaultAsync(s => s.Id == model.FscSerialId);

                if (serial == null)
                    return Json(new { success = false, message = "Bobin bulunamadı." });

                int? oldRecipeId = null;
                decimal oldConsumed = 0, oldWaste = 0, oldQty = 0;

                if (model.Id == 0)
                {
                    // Yeni kayit: stok dusus = tuketilen + fire (ikisi bagimsiz kalem)
                    var totalDeduction = model.ConsumedWeight + model.WasteWeight;
                    if (totalDeduction > serial.CurrentWeight)
                        return Json(new { success = false, message = "Tuketim + fire toplami bobinin kalan agirligini asiyor." });
                    serial.CurrentWeight -= totalDeduction;
                    _context.ProductionDetails.Add(model);
                }
                else
                {
                    var existing = await _context.ProductionDetails.FindAsync(model.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Kayıt bulunamadı." });

                    oldRecipeId = existing.WorkOrderRecipeId;
                    oldConsumed = existing.ConsumedWeight;
                    oldWaste    = existing.WasteWeight;
                    oldQty      = existing.ProducedQuantity;

                    // Eski tüketimi iade et, yenisini düş
                    var diff = (model.ConsumedWeight + model.WasteWeight) - (existing.ConsumedWeight + existing.WasteWeight);
                    if (diff > serial.CurrentWeight)
                        return Json(new { success = false, message =
                            $"Bobinde yeterli stok yok (kalan: {serial.CurrentWeight:N2} kg, gerekli: {diff:N2} kg). " +
                            "Eksikse önce YM Dönüşüm sayfasından dönüşümü tamamlayın, sonra düzeltmeyi tekrar deneyin." });

                    serial.CurrentWeight         -= diff;

                    _context.ProductionDetailAudits.Add(new ProductionDetailAudit
                    {
                        ProductionDetailId  = existing.Id,
                        WorkOrderId         = model.WorkOrderId,
                        Action              = "Edit",
                        Reason              = correctionReason!,
                        OldConsumedWeight   = oldConsumed,
                        OldWasteWeight      = oldWaste,
                        OldProducedQuantity = oldQty,
                        NewConsumedWeight   = model.ConsumedWeight,
                        NewWasteWeight      = model.WasteWeight,
                        NewProducedQuantity = model.ProducedQuantity,
                        ChangedBy           = User.Identity?.Name ?? HttpContext.Session.GetString("Username") ?? "Admin",
                        ChangedDate         = DateTime.Now
                    });
                    existing.FscSerialId         = model.FscSerialId;
                    existing.MachineId           = model.MachineId;
                    existing.ProductionDate      = model.ProductionDate;
                    existing.ConsumedWeight      = model.ConsumedWeight;
                    existing.WasteWeight         = model.WasteWeight;
                    existing.ProducedQuantity    = model.ProducedQuantity;
                    existing.Notes               = model.Notes;
                    existing.WorkOrderRecipeId   = model.WorkOrderRecipeId;
                }

                // İş emrini "Üretimde" durumuna geçir
                // Is emrini 'Uretimde' durumuna gecir
                if (wo.Status == WorkOrderStatus.Taslak)
                    wo.Status = WorkOrderStatus.Uretimde;

                // ── WorkOrderRecipe güncelle (BOM bileşen bazlı toplamlar) ────────
                // Eski reçete satırını güncelle (düzenleme durumu)
                if (oldRecipeId.HasValue && oldRecipeId != model.WorkOrderRecipeId)
                {
                    var oldRecipe = await _context.WorkOrderRecipes.FindAsync(oldRecipeId.Value);
                    if (oldRecipe != null)
                    {
                        oldRecipe.ActualConsumedQuantity = Math.Max(0, oldRecipe.ActualConsumedQuantity - oldConsumed);
                        oldRecipe.WasteQuantity          = Math.Max(0, oldRecipe.WasteQuantity          - oldWaste);
                        oldRecipe.ProducedQuantity       = Math.Max(0, oldRecipe.ProducedQuantity       - oldQty);
                    }
                }

                // Yeni/güncel reçete satırını güncelle
                if (model.WorkOrderRecipeId.HasValue)
                {
                    var recipe = await _context.WorkOrderRecipes.FindAsync(model.WorkOrderRecipeId.Value);
                    if (recipe != null)
                    {
                        // Düzenleme: eski değerleri çıkar, yenileri ekle
                        var prevConsumed = (model.Id > 0 && oldRecipeId == model.WorkOrderRecipeId) ? oldConsumed : 0;
                        var prevWaste    = (model.Id > 0 && oldRecipeId == model.WorkOrderRecipeId) ? oldWaste    : 0;
                        var prevQty      = (model.Id > 0 && oldRecipeId == model.WorkOrderRecipeId) ? oldQty      : 0;

                        recipe.ActualConsumedQuantity += model.ConsumedWeight - prevConsumed;
                        recipe.WasteQuantity          += model.WasteWeight    - prevWaste;
                        // ProducedQuantity biriktirme YAPMA: aynı iş emrindeki her malzeme satırı
                        // aynı üretim adedini taşır. Reçete bileşeni için en son (max) değeri al.
                        recipe.ProducedQuantity        = Math.Max(recipe.ProducedQuantity, model.ProducedQuantity);
                        recipe.FscSerialId             = model.FscSerialId; // son kullanılan bobin
                    }
                }

                await _context.SaveChangesAsync();

                // Stok hareketi: tüketim (çıkış) — tüketilen malzemenin ürünü için. Detay ile ErpReferenceId üzerinden eşlenir.
                if (serial.Lot?.ProductId != null)
                {
                    var consMov = await _context.StockMovements
                        .FirstOrDefaultAsync(sm => sm.Type == MovementType.ProductionConsumption && sm.ErpReferenceId == model.Id);
                    if (consMov == null)
                    {
                        _context.StockMovements.Add(new StockMovement
                        {
                            Type           = MovementType.ProductionConsumption,
                            ErpReferenceId = model.Id,
                            ProductId      = serial.Lot.ProductId.Value,
                            Quantity       = model.ConsumedWeight + model.WasteWeight,
                            Unit           = "kg",
                            DocumentNo     = wo?.WorkOrderNo ?? "",
                            DocumentDate   = model.ProductionDate,
                            WorkOrderId    = model.WorkOrderId,
                            Description    = $"Üretim tüketimi — {wo?.WorkOrderNo}",
                            CreatedBy      = User.Identity?.Name ?? "System",
                            CreatedDate    = DateTime.Now
                        });
                    }
                    else
                    {
                        consMov.Quantity     = model.ConsumedWeight + model.WasteWeight;
                        consMov.ProductId    = serial.Lot.ProductId.Value;
                        consMov.DocumentDate = model.ProductionDate;
                    }
                    await _context.SaveChangesAsync();
                }

                // Üretim firesi → İmha Kayıtları (WasteManagement) tek defter. Detay ile WasteCode üzerinden eşlenir.
                var fireCode = $"FIRE-D{model.Id}";
                var fireRec = await _context.WasteManagements.FirstOrDefaultAsync(w => w.WasteCode == fireCode);
                if (model.WasteWeight > 0)
                {
                    if (fireRec == null)
                    {
                        _context.WasteManagements.Add(new FSCTakip.Core.Entities.WasteManagement
                        {
                            WasteCode      = fireCode,
                            WorkOrderId    = model.WorkOrderId,
                            Category       = WasteCategory.KesimArtigi,
                            Description    = $"Üretim firesi — {wo?.WorkOrderNo}",
                            Quantity       = model.WasteWeight,
                            Unit           = "kg",
                            DisposalDate   = model.ProductionDate,
                            DisposalMethod = "Geri Dönüşüm",
                            CreatedBy      = User.Identity?.Name ?? "System",
                            CreatedDate    = DateTime.Now
                        });
                    }
                    else
                    {
                        fireRec.Quantity     = model.WasteWeight;
                        fireRec.DisposalDate = model.ProductionDate;
                    }
                    await _context.SaveChangesAsync();
                }
                else if (fireRec != null)
                {
                    _context.WasteManagements.Remove(fireRec);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Tüketim kaydedildi.", kalanKg = serial.CurrentWeight });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/DeleteDetail
        [HttpPost]
        public async Task<IActionResult> DeleteDetail(int id, string? correctionReason = null)
        {
            try
            {
                if (!IsAdminUser)
                    return Json(new { success = false, message = "Tüketim kayıtlarını yalnızca admin silebilir." });
                if (string.IsNullOrWhiteSpace(correctionReason))
                    return Json(new { success = false, message = "Silme nedeni zorunludur." });

                var detail = await _context.ProductionDetails
                    .Include(d => d.FscSerial)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (detail == null)
                    return Json(new { success = false, message = "Kayıt bulunamadı." });

                _context.ProductionDetailAudits.Add(new ProductionDetailAudit
                {
                    ProductionDetailId  = detail.Id,
                    WorkOrderId         = detail.WorkOrderId,
                    Action              = "Delete",
                    Reason              = correctionReason!,
                    OldConsumedWeight   = detail.ConsumedWeight,
                    OldWasteWeight      = detail.WasteWeight,
                    OldProducedQuantity = detail.ProducedQuantity,
                    ChangedBy           = HttpContext.Session.GetString("Username") ?? "Admin",
                    ChangedDate         = DateTime.Now
                });

                // Tüketimi iade et
                detail.FscSerial.CurrentWeight += detail.ConsumedWeight + detail.WasteWeight;

                // WorkOrderRecipe toplamlılarını güncelle
                if (detail.WorkOrderRecipeId.HasValue)
                {
                    var recipe = await _context.WorkOrderRecipes.FindAsync(detail.WorkOrderRecipeId.Value);
                    if (recipe != null)
                    {
                        recipe.ActualConsumedQuantity = Math.Max(0, recipe.ActualConsumedQuantity - detail.ConsumedWeight);
                        recipe.WasteQuantity          = Math.Max(0, recipe.WasteQuantity          - detail.WasteWeight);
                        // ProducedQuantity: silme sonrası kalan detayların max'ını hesapla
                        var remainingDetails = await _context.ProductionDetails
                            .Where(d => d.WorkOrderRecipeId == detail.WorkOrderRecipeId && d.Id != detail.Id)
                            .ToListAsync();
                        recipe.ProducedQuantity = remainingDetails.Any()
                            ? remainingDetails.Max(d => d.ProducedQuantity)
                            : 0;
                    }
                }

                // İlgili tüketim (çıkış) stok hareketini de kaldır
                var consMovs = await _context.StockMovements
                    .Where(sm => sm.Type == MovementType.ProductionConsumption && sm.ErpReferenceId == id)
                    .ToListAsync();
                if (consMovs.Count > 0) _context.StockMovements.RemoveRange(consMovs);

                // İlgili üretim firesi imha kaydını da kaldır
                var fireCode = $"FIRE-D{id}";
                var fireRec = await _context.WasteManagements.FirstOrDefaultAsync(w => w.WasteCode == fireCode);
                if (fireRec != null) _context.WasteManagements.Remove(fireRec);

                _context.ProductionDetails.Remove(detail);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tüketim kaydı silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/SaveWorkOrderRecipe — İş emrine reçete satırı ekle/güncelle
        [HttpPost]
        public async Task<IActionResult> SaveWorkOrderRecipe(int workOrderId, int productId, decimal plannedQuantity, int? existingId, string? description = null)
        {
            try
            {
                if (existingId.HasValue)
                {
                    var rec = await _context.WorkOrderRecipes.FindAsync(existingId.Value);
                    if (rec == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
                    rec.PlannedQuantity = plannedQuantity;
                    rec.ProductId       = productId;
                }
                else
                {
                    // Aynı iş emri + bileşen zaten var mı?
                    var exists = await _context.WorkOrderRecipes.AnyAsync(r => r.WorkOrderId == workOrderId && r.ProductId == productId);
                    if (exists)
                        return Json(new { success = false, message = "Bu bileşen zaten bu iş emrinde mevcut." });

                    _context.WorkOrderRecipes.Add(new WorkOrderRecipe {
                        WorkOrderId       = workOrderId,
                        ProductId         = productId,
                        PlannedQuantity   = plannedQuantity,
                        Description       = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                        CreatedDate       = DateTime.Now,
                        CreatedBy         = User.Identity?.Name ?? "System"
                    });
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Reçete satırı kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/DeleteWorkOrderRecipe
        [HttpPost]
        public async Task<IActionResult> DeleteWorkOrderRecipe(int id)
        {
            try
            {
                var rec = await _context.WorkOrderRecipes.FindAsync(id);
                if (rec == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
                if (rec.ActualConsumedQuantity > 0)
                    return Json(new { success = false, message = "Bu bileşene bağlı tüketim kaydı var. Önce tüketim kayıtlarını silin." });
                _context.WorkOrderRecipes.Remove(rec);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
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
        public async Task<IActionResult> WasteReport(DateTime? startDate, DateTime? endDate, int? machineId, int[]? productIds)
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
            if (productIds != null && productIds.Length > 0)
                query = query.Where(d => productIds.Contains(d.WorkOrder.ProductId));

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
            ViewBag.ProductIds = productIds ?? Array.Empty<int>();
            ViewData["Title"]  = "Fire Raporu";
            return View(details);
        }

        // GET /Production/ExportWasteReport
        public async Task<IActionResult> ExportWasteReport(DateTime? startDate, DateTime? endDate, int? machineId, int[]? productIds)
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
            if (productIds != null && productIds.Length > 0)
                query = query.Where(d => productIds.Contains(d.WorkOrder.ProductId));

            var data = await query.OrderByDescending(d => d.ProductionDate).ToListAsync();
            var rows = data.Select(d => {
                var fr = d.ConsumedWeight > 0 ? Math.Round(d.WasteWeight / d.ConsumedWeight * 100, 2) : 0;
                return new {
                    Tarih        = d.ProductionDate.ToString("dd.MM.yyyy"),
                    IsEmriNo     = d.WorkOrder?.WorkOrderNo,
                    Urun         = d.WorkOrder?.Product?.ProductName,
                    SerialNo     = d.FscSerial?.SerialNo,
                    PartiNo      = d.FscSerial?.Lot?.PartiNo,
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

        // POST /Production/AssignDetailRecipe — sadece bileşen atamasını değiştirir, iş emri durumundan bağımsız
        [HttpPost]
        public async Task<IActionResult> AssignDetailRecipe(int detailId, int? workOrderRecipeId)
        {
            var detail = await _context.ProductionDetails.FindAsync(detailId);
            if (detail == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            detail.WorkOrderRecipeId = workOrderRecipeId == 0 ? null : workOrderRecipeId;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWaste(int id)
        {
            var item = await _context.WasteManagements.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
            try
            {
                _context.WasteManagements.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (FSCTakip.Core.Entities.PeriodLockedException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Bu atık kaydı silinemez. {ex.Message}" });
            }
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

