using Microsoft.AspNetCore.Mvc;

namespace FSC_ERP.Controllers
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
