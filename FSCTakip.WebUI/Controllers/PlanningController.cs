using Microsoft.AspNetCore.Mvc;

namespace FSC_ERP.Controllers
{
    public class PlanningController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Üretim Planlama";
            return View();
        }
    }
}
