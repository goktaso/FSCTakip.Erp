using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    [Authorize]
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

                wo.Status        = WorkOrderStatus.Tamamlandi;
                wo.CompletedDate = DateTime.Now;
                // Üretilen adet: aynı gün içindeki tüm malzeme satırları aynı adeti taşır.
                // Her tarih için Max alıp günleri topla → çok günlü üretimi de doğru hesaplar.
                var prodDetails = await _context.ProductionDetails
                    .Where(d => d.WorkOrderId == id)
                    .ToListAsync();
                wo.ActualQuantity = prodDetails.Any()
                    ? prodDetails
                        .GroupBy(d => d.ProductionDate.Date)
                        .Sum(g => g.Max(d => d.ProducedQuantity))
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
            return Json(new { success = true, message = $"{updated} iş emrinin üretim adedi güncellendi." });
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
                .Where(s => s.CurrentWeight > 0)
                .OrderBy(s => s.SerialNo)
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
            ViewData["Title"] = $"İş Emri — {wo.WorkOrderNo}";
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
        public async Task<IActionResult> SaveDetail(ProductionDetail model)
        {
            try
            {
                if (model.ConsumedWeight <= 0)
                    return Json(new { success = false, message = "Tüketim miktarı sıfırdan büyük olmalıdır." });
                if (model.WasteWeight < 0)
                    return Json(new { success = false, message = "Fire miktarı negatif olamaz." });
                if (model.ProducedQuantity <= 0)
                    return Json(new { success = false, message = "Üretilen adet sıfırdan büyük olmalıdır." });
                if (model.WasteWeight > model.ConsumedWeight)
                    return Json(new { success = false, message = "Fire miktarı tüketim miktarını aşamaz." });

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

                    oldRecipeId = existing.WorkOrderRecipeId;
                    oldConsumed = existing.ConsumedWeight;
                    oldWaste    = existing.WasteWeight;
                    oldQty      = existing.ProducedQuantity;

                    // Eski tüketimi iade et, yenisini düş
                    var diff = model.ConsumedWeight - existing.ConsumedWeight;
                    if (diff > serial.CurrentWeight)
                        return Json(new { success = false, message = $"Güncellenmiş tüketim bobinin kalan ağırlığını aşıyor." });

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

                // İş emrini "Üretimde" durumuna geçir
                var wo = await _context.WorkOrders.FindAsync(model.WorkOrderId);
                if (wo != null && wo.Status == WorkOrderStatus.Taslak)
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
                            Quantity       = model.ConsumedWeight,
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
                        consMov.Quantity     = model.ConsumedWeight;
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
        public async Task<IActionResult> SaveWorkOrderRecipe(int workOrderId, int productId, decimal plannedQuantity, int? existingId)
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
