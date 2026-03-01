using Microsoft.AspNetCore.Mvc;

namespace FSCTakip.Web.Controllers
{
    public class ProductsController : Controller
    {
        // localhost:xxxx/Products dediğinde burası çalışır
        public IActionResult Index()
        {
            return View();
        }

        // Yeni Ürün Ekleme sayfası veya işlemi için
        public IActionResult Create()
        {
            return View();
        }
    }
}