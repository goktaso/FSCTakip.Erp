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
                    if (existing == null) return Json(new { success = false, message = "Ä°ÅŸ emri bulunamadÄ±." });
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
                return Json(new { success = true, message = "Ä°ÅŸ emri kaydedildi.", workOrderNo = model.WorkOrderNo, workOrderId = model.Id });
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

                if (wo == null) return Json(new { success = false, message = "Ä°ÅŸ emri bulunamadÄ±." });
                if (wo.ProductionDetails.Any())
                    return Json(new { success = false, message = "Bu iÅŸ emrine ait Ã¼retim kaydÄ± var, silinemez." });

                _context.WorkOrders.Remove(wo);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ä°ÅŸ emri silindi." });
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
                // Ãœretilen adet: aynÄ± gÃ¼n iÃ§indeki tÃ¼m malzeme satÄ±rlarÄ± aynÄ± adeti taÅŸÄ±r.
                // Her tarih iÃ§in Max alÄ±p gÃ¼nleri topla â†’ Ã§ok gÃ¼nlÃ¼ Ã¼retimi de doÄŸru hesaplar.
                var prodDetails = await _context.ProductionDetails
                    .Where(d => d.WorkOrderId == id)
                    .ToListAsync();
                wo.ActualQuantity = prodDetails.Any()
                    ? prodDetails
                        .GroupBy(d => d.ProductionDate.Date)
                        .Sum(g => g.Max(d => d.ProducedQuantity))
                    : 0;

                // Mamul â†’ bitmiÅŸ Ã¼rÃ¼n stoÄŸuna giriÅŸ (ProductionEntry). Ä°ÅŸ emriyle eÅŸlenir; yeniden tamamlamada gÃ¼ncellenir.
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
                            Description  = $"Ãœretimden giriÅŸ â€” {wo.WorkOrderNo}",
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
                return Json(new { success = true, message = "Ä°ÅŸ emri tamamlandÄ± olarak iÅŸaretlendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/RecalcAllActualQty â€” Mevcut yanlÄ±ÅŸ ActualQuantity deÄŸerlerini dÃ¼zelt (admin)
        [HttpPost]
        public async Task<IActionResult> RecalcAllActualQty()
        {
            if (!IsAdminUser)
                return Json(new { success = false, message = "Bu iÅŸlem yalnÄ±zca admin tarafÄ±ndan yapÄ±labilir." });

            var workOrders = await _context.WorkOrders
                .Where(w => w.Status != WorkOrderStatus.Taslak)
                .ToListAsync();

            var allDetails = await _context.ProductionDetails.ToListAsync();
            var detailsByWo = allDetails.GroupBy(d => d.WorkOrderId)
                              .ToDictionary(g => g.Key, g => g.ToList());

            int updated = 0;
            foreach (var wo in workOrders)
            {
                if (!detailsByWo.TryGetValue(wo.Id, out var details) || !details.Any()) continue;
                var correct = details
                    .GroupBy(d => d.ProductionDate.Date)
                    .Sum(g => g.Max(d => d.ProducedQuantity));
                if (wo.ActualQuantity != correct)
                {
                    wo.ActualQuantity = correct;
                    updated++;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{updated} iÅŸ emrinin Ã¼retim adedi gÃ¼ncellendi." });
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

            // ÃœrÃ¼nÃ¼n tanÄ±mlÄ± reÃ§ete bileÅŸenleri (BOM dropdown iÃ§in)
            var recipeComponents = await _context.ProductRecipes
                .Include(r => r.ChildProduct).ThenInclude(p => p.FscType)
                .Where(r => r.ParentProductId == wo.ProductId && r.IsActive)
                .OrderBy(r => r.ChildProduct.ProductName)
                .ToListAsync();

            // WorkOrderRecipes â€” mevcut reÃ§ete satÄ±rlarÄ± (varsa)
            var workOrderRecipes = wo.WorkOrderRecipes.OrderBy(r => r.Product?.ProductName).ToList();

            ViewBag.AvailableSerials   = availableSerials;
            ViewBag.Machines           = await _context.Machines.OrderBy(m => m.Name).ToListAsync();
            ViewBag.RecipeComponents   = recipeComponents;
            ViewBag.WorkOrderRecipes   = workOrderRecipes;
            ViewData["Title"] = $"Ä°ÅŸ Emri â€” {wo.WorkOrderNo}";
            return View(wo);
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

            if (serial == null) return Json(new { success = false, message = "Bobin bulunamadÄ±." });

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
        public async Task<IActionResult> SaveDetail(ProductionDetail model)
        {
            try
            {
                if (model.ConsumedWeight <= 0)
                    return Json(new { success = false, message = "TÃ¼ketim miktarÄ± sÄ±fÄ±rdan bÃ¼yÃ¼k olmalÄ±dÄ±r." });
                if (model.WasteWeight < 0)
                    return Json(new { success = false, message = "Fire miktarÄ± negatif olamaz." });
                if (model.ProducedQuantity <= 0)
                    return Json(new { success = false, message = "Ãœretilen adet sÄ±fÄ±rdan bÃ¼yÃ¼k olmalÄ±dÄ±r." });
                if (model.WasteWeight > model.ConsumedWeight)
                    return Json(new { success = false, message = "Fire miktarÄ± tÃ¼ketim miktarÄ±nÄ± aÅŸamaz." });

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
                    return Json(new { success = false, message = "Bobin bulunamadÄ±." });

                int? oldRecipeId = null;
                decimal oldConsumed = 0, oldWaste = 0, oldQty = 0;

                if (model.Id == 0)
                {
                    // Yeni kayÄ±t: stok dÃ¼ÅŸ
                    if (model.ConsumedWeight > serial.CurrentWeight)
                        return Json(new { success = false, message = $"TÃ¼ketim miktarÄ± ({model.ConsumedWeight:N2} kg) bobinin kalan aÄŸÄ±rlÄ±ÄŸÄ±nÄ± ({serial.CurrentWeight:N2} kg) aÅŸÄ±yor." });

                    serial.CurrentWeight -= model.ConsumedWeight;
                    _context.ProductionDetails.Add(model);
                }
                else
                {
                    var existing = await _context.ProductionDetails.FindAsync(model.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "KayÄ±t bulunamadÄ±." });

                    oldRecipeId = existing.WorkOrderRecipeId;
                    oldConsumed = existing.ConsumedWeight;
                    oldWaste    = existing.WasteWeight;
                    oldQty      = existing.ProducedQuantity;

                    // Eski tÃ¼ketimi iade et, yenisini dÃ¼ÅŸ
                    var diff = model.ConsumedWeight - existing.ConsumedWeight;
                    if (diff > serial.CurrentWeight)
                        return Json(new { success = false, message = $"GÃ¼ncellenmiÅŸ tÃ¼ketim bobinin kalan aÄŸÄ±rlÄ±ÄŸÄ±nÄ± aÅŸÄ±yor." });

                    serial.CurrentWeight         -= diff;
                    existing.FscSerialId         = model.FscSerialId;
                    existing.MachineId           = model.MachineId;
                    existing.ProductionDate      = model.ProductionDate;
                    existing.ConsumedWeight      = model.ConsumedWeight;
                    existing.WasteWeight         = model.WasteWeight;
                    existing.ProducedQuantity    = model.ProducedQuantity;
                    existing.Notes               = model.Notes;
                    existing.WorkOrderRecipeId   = model.WorkOrderRecipeId;
                }

                // Ä°ÅŸ emrini "Ãœretimde" durumuna geÃ§ir
                // Is emrini 'Uretimde' durumuna gecir
                if (wo.Status == WorkOrderStatus.Taslak)
                    wo.Status = WorkOrderStatus.Uretimde;

                // â”€â”€ WorkOrderRecipe gÃ¼ncelle (BOM bileÅŸen bazlÄ± toplamlar) â”€â”€â”€â”€â”€â”€â”€â”€
                // Eski reÃ§ete satÄ±rÄ±nÄ± gÃ¼ncelle (dÃ¼zenleme durumu)
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

                // Yeni/gÃ¼ncel reÃ§ete satÄ±rÄ±nÄ± gÃ¼ncelle
                if (model.WorkOrderRecipeId.HasValue)
                {
                    var recipe = await _context.WorkOrderRecipes.FindAsync(model.WorkOrderRecipeId.Value);
                    if (recipe != null)
                    {
                        // DÃ¼zenleme: eski deÄŸerleri Ã§Ä±kar, yenileri ekle
                        var prevConsumed = (model.Id > 0 && oldRecipeId == model.WorkOrderRecipeId) ? oldConsumed : 0;
                        var prevWaste    = (model.Id > 0 && oldRecipeId == model.WorkOrderRecipeId) ? oldWaste    : 0;
                        var prevQty      = (model.Id > 0 && oldRecipeId == model.WorkOrderRecipeId) ? oldQty      : 0;

                        recipe.ActualConsumedQuantity += model.ConsumedWeight - prevConsumed;
                        recipe.WasteQuantity          += model.WasteWeight    - prevWaste;
                        // ProducedQuantity biriktirme YAPMA: aynÄ± iÅŸ emrindeki her malzeme satÄ±rÄ±
                        // aynÄ± Ã¼retim adedini taÅŸÄ±r. ReÃ§ete bileÅŸeni iÃ§in en son (max) deÄŸeri al.
                        recipe.ProducedQuantity        = Math.Max(recipe.ProducedQuantity, model.ProducedQuantity);
                        recipe.FscSerialId             = model.FscSerialId; // son kullanÄ±lan bobin
                    }
                }

                await _context.SaveChangesAsync();

                // Stok hareketi: tÃ¼ketim (Ã§Ä±kÄ±ÅŸ) â€” tÃ¼ketilen malzemenin Ã¼rÃ¼nÃ¼ iÃ§in. Detay ile ErpReferenceId Ã¼zerinden eÅŸlenir.
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
                            Quantity       = model.ConsumedWeight,
                            Unit           = "kg",
                            DocumentNo     = wo?.WorkOrderNo ?? "",
                            DocumentDate   = model.ProductionDate,
                            WorkOrderId    = model.WorkOrderId,
                            Description    = $"Ãœretim tÃ¼ketimi â€” {wo?.WorkOrderNo}",
                            CreatedBy      = User.Identity?.Name ?? "System",
                            CreatedDate    = DateTime.Now
                        });
                    }
                    else
                    {
                        consMov.Quantity     = model.ConsumedWeight;
                        consMov.ProductId    = serial.Lot.ProductId.Value;
                        consMov.DocumentDate = model.ProductionDate;
                    }
                    await _context.SaveChangesAsync();
                }

                // Ãœretim firesi â†’ Ä°mha KayÄ±tlarÄ± (WasteManagement) tek defter. Detay ile WasteCode Ã¼zerinden eÅŸlenir.
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
                            Description    = $"Ãœretim firesi â€” {wo?.WorkOrderNo}",
                            Quantity       = model.WasteWeight,
                            Unit           = "kg",
                            DisposalDate   = model.ProductionDate,
                            DisposalMethod = "Geri DÃ¶nÃ¼ÅŸÃ¼m",
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

                return Json(new { success = true, message = "TÃ¼ketim kaydedildi.", kalanKg = serial.CurrentWeight });
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
                    return Json(new { success = false, message = "KayÄ±t bulunamadÄ±." });

                // TÃ¼ketimi iade et
                detail.FscSerial.CurrentWeight += detail.ConsumedWeight;

                // WorkOrderRecipe toplamlÄ±larÄ±nÄ± gÃ¼ncelle
                if (detail.WorkOrderRecipeId.HasValue)
                {
                    var recipe = await _context.WorkOrderRecipes.FindAsync(detail.WorkOrderRecipeId.Value);
                    if (recipe != null)
                    {
                        recipe.ActualConsumedQuantity = Math.Max(0, recipe.ActualConsumedQuantity - detail.ConsumedWeight);
                        recipe.WasteQuantity          = Math.Max(0, recipe.WasteQuantity          - detail.WasteWeight);
                        // ProducedQuantity: silme sonrasÄ± kalan detaylarÄ±n max'Ä±nÄ± hesapla
                        var remainingDetails = await _context.ProductionDetails
                            .Where(d => d.WorkOrderRecipeId == detail.WorkOrderRecipeId && d.Id != detail.Id)
                            .ToListAsync();
                        recipe.ProducedQuantity = remainingDetails.Any()
                            ? remainingDetails.Max(d => d.ProducedQuantity)
                            : 0;
                    }
                }

                // Ä°lgili tÃ¼ketim (Ã§Ä±kÄ±ÅŸ) stok hareketini de kaldÄ±r
                var consMovs = await _context.StockMovements
                    .Where(sm => sm.Type == MovementType.ProductionConsumption && sm.ErpReferenceId == id)
                    .ToListAsync();
                if (consMovs.Count > 0) _context.StockMovements.RemoveRange(consMovs);

                // Ä°lgili Ã¼retim firesi imha kaydÄ±nÄ± da kaldÄ±r
                var fireCode = $"FIRE-D{id}";
                var fireRec = await _context.WasteManagements.FirstOrDefaultAsync(w => w.WasteCode == fireCode);
                if (fireRec != null) _context.WasteManagements.Remove(fireRec);

                _context.ProductionDetails.Remove(detail);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "TÃ¼ketim kaydÄ± silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST /Production/SaveWorkOrderRecipe â€” Ä°ÅŸ emrine reÃ§ete satÄ±rÄ± ekle/gÃ¼ncelle
        [HttpPost]
        public async Task<IActionResult> SaveWorkOrderRecipe(int workOrderId, int productId, decimal plannedQuantity, int? existingId, string? description = null)
        {
            try
            {
                if (existingId.HasValue)
                {
                    var rec = await _context.WorkOrderRecipes.FindAsync(existingId.Value);
                    if (rec == null) return Json(new { success = false, message = "KayÄ±t bulunamadÄ±." });
                    rec.PlannedQuantity = plannedQuantity;
                    rec.ProductId       = productId;
                }
                else
                {
                    // AynÄ± iÅŸ emri + bileÅŸen zaten var mÄ±?
                    var exists = await _context.WorkOrderRecipes.AnyAsync(r => r.WorkOrderId == workOrderId && r.ProductId == productId);
                    if (exists)
                        return Json(new { success = false, message = "Bu bileÅŸen zaten bu iÅŸ emrinde mevcut." });

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
                return Json(new { success = true, message = "ReÃ§ete satÄ±rÄ± kaydedildi." });
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
                if (rec == null) return Json(new { success = false, message = "KayÄ±t bulunamadÄ±." });
                if (rec.ActualConsumedQuantity > 0)
                    return Json(new { success = false, message = "Bu bileÅŸene baÄŸlÄ± tÃ¼ketim kaydÄ± var. Ã–nce tÃ¼ketim kayÄ±tlarÄ±nÄ± silin." });
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

            // Makine bazlÄ± Ã¶zet
            var byMachine = details
                .GroupBy(d => new { d.MachineId, Name = d.Machine?.Name ?? "â€”" })
                .Select(g => new WasteGroupRow {
                    Label          = g.Key.Name,
                    RecordCount    = g.Count(),
                    TotalConsumed  = g.Sum(d => d.ConsumedWeight),
                    TotalWaste     = g.Sum(d => d.WasteWeight)
                })
                .OrderByDescending(r => r.WasteRate)
                .ToList();

            // ÃœrÃ¼n bazlÄ± Ã¶zet
            var byProduct = details
                .GroupBy(d => new { Id = d.WorkOrder?.ProductId, Name = d.WorkOrder?.Product?.ProductName ?? "â€”" })
                .Select(g => new WasteGroupRow {
                    Label         = g.Key.Name,
                    RecordCount   = g.Count(),
                    TotalConsumed = g.Sum(d => d.ConsumedWeight),
                    TotalWaste    = g.Sum(d => d.WasteWeight)
                })
                .OrderByDescending(r => r.WasteRate)
                .ToList();

            // AylÄ±k trend (son 6 ay)
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

        // â”€â”€â”€ WasteManagement (Ä°mha KayÄ±tlarÄ±) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // GET /Production/WasteManagement
        public async Task<IActionResult> WasteManagement()
        {
            ViewData["Title"] = "Ä°mha / AtÄ±k KayÄ±tlarÄ±";
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
                    if (existing == null) return Json(new { success = false, message = "KayÄ±t bulunamadÄ±." });

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
                return Json(new { success = true, message = "KayÄ±t kaydedildi." });
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
                return Json(new { success = false, message = $"Bu atÄ±k kaydÄ± silinemez. {ex.Message}" });
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

