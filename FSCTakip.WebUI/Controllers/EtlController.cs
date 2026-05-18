using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    public class EtlController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "ERP Entegrasyonu";
            return View();
        }
    }
}
