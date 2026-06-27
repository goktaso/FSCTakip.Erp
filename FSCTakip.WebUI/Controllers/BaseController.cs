using FSCTakip.Business.Services;
using FSCTakip.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;
using System.IO;

namespace FSCTakip.WebUI.Controllers
{
    public class BaseController : Controller
    {
        protected readonly AppDbContext _context;

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        // ── Yetki Kontrol Yardımcıları ────────────────────────────────────────

        protected int CurrentUserId =>
            int.TryParse(HttpContext.Session.GetString("UserId"), out var id) ? id : 0;

        protected bool IsAdminUser =>
            HttpContext.Session.GetString("IsAdmin") == "1";

        /// <summary>Session önbelleğinden yetki okur — her request'te DB'ye gitme.</summary>
        protected EffectivePermission GetPermission(string moduleCode)
        {
            if (IsAdminUser) return EffectivePermission.All;
            var json = HttpContext.Session.GetString("Perms");
            if (string.IsNullOrEmpty(json)) return EffectivePermission.None;
            try
            {
                var perms = System.Text.Json.JsonSerializer.Deserialize<
                    Dictionary<string, System.Text.Json.JsonElement>>(json);
                if (perms != null && perms.TryGetValue(moduleCode, out var perm))
                    return new EffectivePermission
                    {
                        CanRead   = perm.GetProperty("r").GetBoolean(),
                        CanWrite  = perm.GetProperty("w").GetBoolean(),
                        CanDelete = perm.GetProperty("d").GetBoolean()
                    };
            }
            catch { }
            return EffectivePermission.None;
        }

        /// <summary>Yazma yetkisi yoksa 403 JSON döner.</summary>
        protected IActionResult? RequireWrite(string moduleCode)
        {
            if (!GetPermission(moduleCode).CanWrite)
                return Json(new { success = false, message = "Bu işlem için yetkiniz bulunmamaktadır." });
            return null;
        }

        /// <summary>Silme yetkisi yoksa 403 JSON döner.</summary>
        protected IActionResult? RequireDelete(string moduleCode)
        {
            if (!GetPermission(moduleCode).CanDelete)
                return Json(new { success = false, message = "Silme yetkisine sahip değilsiniz." });
            return null;
        }

        /// <summary>Okuma yetkisi yoksa AccessDenied view döner.</summary>
        protected IActionResult? RequireRead(string moduleCode)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            if (!GetPermission(moduleCode).CanRead)
                return View("~/Views/Shared/AccessDenied.cshtml");
            return null;
        }

        /// <summary>View'a yetki bayraklarını ViewBag ile iletir.</summary>
        protected void SetPermissionViewBag(string moduleCode)
        {
            var p = GetPermission(moduleCode);
            ViewBag.CanRead   = p.CanRead;
            ViewBag.CanWrite  = p.CanWrite;
            ViewBag.CanDelete = p.CanDelete;
        }

        // 1. GENEL AKTİF/PASİF YAPMA METODU
        [HttpPost]
        public async Task<IActionResult> GeneralToggleStatus(string tableName, int id)
        {
            try
            {
                string assemblyName = "FSCTakip.Core";
                string fullTypeName = $"FSCTakip.Core.Entities.{tableName}, {assemblyName}";
                var type = Type.GetType(fullTypeName);

                if (type == null)
                    return Json(new { success = false, message = $"Sistem '{tableName}' tipini bulamadı!" });

                var entity = await _context.FindAsync(type, id);

                if (entity != null)
                {
                    var property = entity.GetType().GetProperty("IsActive");
                    if (property != null)
                    {
                        bool currentStatus = (bool)property.GetValue(entity);
                        property.SetValue(entity, !currentStatus);
                        await _context.SaveChangesAsync();

                        string durum = !(currentStatus) ? "Aktif" : "Pasif";
                        return Json(new { success = true, message = $"Durum {durum} olarak güncellendi." });
                    }
                }
                return Json(new { success = false, message = "IsActive sütunu bulunamadı!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // 2. CLOSEDXML İLE GERÇEK EXCEL ÇIKTISI (.xlsx)
        // Bu metot PaperController'daki hatayı çözecek olan kısımdır.
        protected IActionResult ExportToExcel<T>(IEnumerable<T> data, string fileName)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(fileName);

                var props = typeof(T).GetProperties()
                    .Where(p => p.PropertyType.IsPrimitive ||
                                p.PropertyType == typeof(string) ||
                                p.PropertyType == typeof(decimal) ||
                                p.PropertyType == typeof(int) ||
                                p.PropertyType == typeof(DateTime) ||
                                p.PropertyType == typeof(double))
                    .ToList();

                // BAŞLIKLAR
                for (int i = 0; i < props.Count; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = props[i].Name;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#343a40");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                // VERİLER
                int currentRow = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < props.Count; i++)
                    {
                        var cell = worksheet.Cell(currentRow, i + 1);
                        var val  = props[i].GetValue(item);
                        if      (val is decimal d)    cell.Value = (double)d;
                        else if (val is int iv)        cell.Value = iv;
                        else if (val is long lv)       cell.Value = (double)lv;
                        else if (val is double dv)     cell.Value = dv;
                        else if (val is float fv)      cell.Value = (double)fv;
                        else if (val is DateTime dt)   cell.Value = dt;
                        else                           cell.Value = val?.ToString() ?? "";
                    }
                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}_{DateTime.Now:ddMMyyyy}.xlsx");
                }
            }
        }

        // 3. CSV ÇIKTISI (Yedek)
        protected IActionResult ExportToCsv<T>(IEnumerable<T> data, string fileName)
        {
            var builder = new StringBuilder();
            var props = typeof(T).GetProperties().Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal)).ToList();
            builder.AppendLine(string.Join(";", props.Select(p => p.Name)));
            foreach (var item in data)
                builder.AppendLine(string.Join(";", props.Select(p => p.GetValue(item)?.ToString()?.Replace(";", ",") ?? "")));

            var result = Encoding.UTF8.GetBytes(builder.ToString());
            var encodedResult = Encoding.UTF8.GetPreamble().Concat(result).ToArray();
            return File(encodedResult, "text/csv", $"{fileName}.csv");
        }
    }
}