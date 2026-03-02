using Microsoft.AspNetCore.Mvc;

namespace FSC_ERP.Controllers
{
    public class PaperController : Controller
    {
        // 1. Kağıt Tipleri (Kraft, Sülfit vb.)
        public IActionResult Types()
        {
            ViewData["Title"] = "Kağıt Tipleri";
            return View();
        }

        // 2. Kağıt Renkleri (Beyaz, Kahve vb.)
        public IActionResult Colors()
        {
            ViewData["Title"] = "Kağıt Renkleri";
            return View();
        }

        // 3. FSC Sertifika Tipleri (100%, Mix, Recycled vb.)
        public IActionResult FscTypes()
        {
            ViewData["Title"] = "FSC Sertifika Tipleri";
            return View();
        }

        // 4. Bobin En Tanımları (mm bazlı: 70mm, 80mm vb.)
        public IActionResult Widths()
        {
            ViewData["Title"] = "Bobin En Tanımları";
            return View();
        }

        // 5. Gramaj Tanımları (70gr, 80gr vb.)
        public IActionResult Weights()
        {
            ViewData["Title"] = "Gramaj Tanımları";
            return View();
        }
    }
}