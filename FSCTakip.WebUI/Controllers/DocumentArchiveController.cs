using FSCTakip.Core.Entities;
using FSCTakip.DataAccess.Data;
using FSCTakip.WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FSCTakip.WebUI.Controllers
{
    public class DocumentArchiveController : BaseController
    {
        private readonly IFileStorageService _storage;
        private readonly IWebHostEnvironment _env;

        public DocumentArchiveController(AppDbContext context, IFileStorageService storage, IWebHostEnvironment env)
            : base(context)
        {
            _storage = storage;
            _env = env;
        }

        // GET /DocumentArchive
        public async Task<IActionResult> Index(DocumentCategory? category, int? year, string? search)
        {
            var query = _context.FscDocuments.AsQueryable();
            if (category.HasValue) query = query.Where(d => d.Category == category.Value);
            if (year.HasValue)     query = query.Where(d => d.Year == year.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(d => d.Title.Contains(search) || (d.Tags != null && d.Tags.Contains(search)));

            ViewBag.Category = category;
            ViewBag.Year     = year;
            ViewBag.Search   = search;
            ViewBag.Years    = await _context.FscDocuments.Select(d => d.Year).Distinct().OrderByDescending(y => y).ToListAsync();
            ViewBag.Counts   = await _context.FscDocuments
                .GroupBy(d => d.Category)
                .Select(g => new { Cat = g.Key, Count = g.Count() })
                .ToListAsync();

            return View(await query.OrderByDescending(d => d.Year).ThenBy(d => d.Category).ThenBy(d => d.Title).ToListAsync());
        }

        // POST /DocumentArchive/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(string title, DocumentCategory category, int year, string? notes, string? tags, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Dosya seçilmedi." });
                if (file.Length > 52_428_800)
                    return Json(new { success = false, message = "Dosya 50 MB sınırını aşıyor." });
                if (string.IsNullOrWhiteSpace(title))
                    return Json(new { success = false, message = "Belge başlığı zorunludur." });

                var ext  = Path.GetExtension(file.FileName).ToLowerInvariant();
                var safe = Path.GetFileNameWithoutExtension(file.FileName)
                               .Replace(" ", "_")
                               .Replace("ı","i").Replace("ş","s").Replace("ğ","g")
                               .Replace("ü","u").Replace("ö","o").Replace("ç","c")
                               + "_" + DateTime.Now.ToString("yyyyMMddHHmm") + ext;

                var catFolder = category.ToString().ToLower();
                var relDir    = Path.Combine("uploads", "archive", catFolder);
                var absDir    = Path.Combine(_env.WebRootPath, "uploads", "archive", catFolder);
                Directory.CreateDirectory(absDir);

                var absPath = Path.Combine(absDir, safe);
                await using (var fs = new FileStream(absPath, FileMode.Create))
                    await file.CopyToAsync(fs);

                var doc = new FscDocument
                {
                    Title         = title.Trim(),
                    Category      = category,
                    Year          = year,
                    FileName      = file.FileName,
                    FilePath      = Path.Combine(relDir, safe).Replace("\\", "/"),
                    FileSize      = file.Length,
                    FileExtension = ext,
                    Notes         = notes,
                    Tags          = tags,
                    CreatedBy     = User.Identity?.Name ?? "System",
                    CreatedDate   = DateTime.Now
                };
                _context.FscDocuments.Add(doc);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Belge yüklendi.", id = doc.Id });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // GET /DocumentArchive/Serve/{id}
        [HttpGet]
        public async Task<IActionResult> Serve(int id)
        {
            var doc = await _context.FscDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            var abs = Path.Combine(_env.WebRootPath, doc.FilePath.Replace("/", "\\"));
            if (!System.IO.File.Exists(abs)) return NotFound("Dosya bulunamadı.");

            var mime = doc.FileExtension switch
            {
                ".pdf"  => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc"  => "application/msword",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls"  => "application/vnd.ms-excel",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _       => "application/octet-stream"
            };

            var isViewable = new[] { ".pdf", ".jpg", ".jpeg", ".png" }.Contains(doc.FileExtension);
            if (isViewable)
                return PhysicalFile(abs, mime);
            else
                return PhysicalFile(abs, mime, doc.FileName);
        }

        // POST /DocumentArchive/Delete/{id}
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.FscDocuments.FindAsync(id);
            if (doc == null) return Json(new { success = false, message = "Belge bulunamadı." });

            try
            {
                var abs = Path.Combine(_env.WebRootPath, doc.FilePath.Replace("/", "\\"));
                if (System.IO.File.Exists(abs)) System.IO.File.Delete(abs);
                _context.FscDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Belge silindi." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}
