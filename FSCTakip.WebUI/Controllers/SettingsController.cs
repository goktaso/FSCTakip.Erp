using Microsoft.AspNetCore.Mvc;

namespace FSC_ERP.Controllers
{
    public class SettingsController : Controller
    {
        // --- ÜRETİM TANIMLARI ---

        public IActionResult Machines()
        {
            ViewData["Title"] = "Makine Tanımlamaları";
            return View();
        }

        public IActionResult ProductGroups()
        {
            ViewData["Title"] = "Ürün Grupları";
            return View();
        }

        public IActionResult BagTypes()
        {
            ViewData["Title"] = "Torba Tipleri";
            return View();
        }

        // --- KAĞIT VE SERTİFİKA AYARLARI ---

        public IActionResult PaperTypes()
        {
            ViewData["Title"] = "Kağıt Tipleri";
            return View();
        }

        public IActionResult PaperColors()
        {
            ViewData["Title"] = "Kağıt Renkleri";
            return View();
        }

        // YENİ: FSC Sertifika Tipleri (100%, Mix, vb.)
        public IActionResult FscTypes()
        {
            ViewData["Title"] = "FSC Sertifika Tipleri";
            return View();
        }

        // YENİ: Bobin En Tanımları (mm bazlı)
        public IActionResult BobinEnleri()
        {
            ViewData["Title"] = "Bobin En Tanımları";
            return View();
        }

        // YENİ: Gramaj Tanımları
        public IActionResult Grammages()
        {
            ViewData["Title"] = "Gramaj Tanımları";
            return View();
        }
    }
}