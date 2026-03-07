using Microsoft.AspNetCore.Mvc;

namespace FSC_ERP.Controllers
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Müşteriler";
            return View();
        }
    }
}
