using Microsoft.AspNetCore.Mvc;

namespace FSC_ERP.Controllers
{
    public class ShippingController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Sevkiyat Paneli";
            return View();
        }
    }
}
