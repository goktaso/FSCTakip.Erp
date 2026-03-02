using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    public class ProductController : Controller
    {
        // Ürün Grupları (Örn: Saplı Çanta, Flat Bag, Ekmek Kesesi vb.)
        public IActionResult Groups()
        {
            ViewData["Title"] = "Ürün Grupları";
            return View();
        }

        // Torba Tipleri (Örn: V Kesim, Dip Takviyeli, Pencereli vb.)
        public IActionResult BagTypes()
        {
            ViewData["Title"] = "Torba Tipleri";
            return View();
        }
    }
}