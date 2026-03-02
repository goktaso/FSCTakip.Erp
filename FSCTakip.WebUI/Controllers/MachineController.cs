using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.WebUI.Controllers
{
    public class MachineController : Controller
    {
        // --- ÜRETİM TANIMLARI ---

        public IActionResult Machines()
        {
            ViewData["Title"] = "Makine Tanımlamaları";
            return View();
        }
    }
}
