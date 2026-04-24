using Microsoft.AspNetCore.Mvc;

namespace SYSGES_MAGs.Controllers
{
    public class CartesClient : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
