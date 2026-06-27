using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    public class GuideController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public GuideController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult TestScenario()
        {
            ViewData["Title"] = "Test Senaryosu";
            return View();
        }

        public IActionResult Index(string? section)
        {
            ViewData["Title"] = "Kullanım Kılavuzu";
            ViewBag.Section = section ?? "giris";

            var mdPath = Path.Combine(_env.ContentRootPath, "..", "docs", "KULLANIM_KILAVUZU.md");
            var mdContent = System.IO.File.Exists(mdPath)
                ? System.IO.File.ReadAllText(mdPath)
                : "Kılavuz dosyası bulunamadı.";

            ViewBag.MarkdownContent = mdContent;
            return View();
        }
    }
}
