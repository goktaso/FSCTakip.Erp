using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Stok Yönetimi";
            return View();
        }
    }
}
