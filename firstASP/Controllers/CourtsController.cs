
using Microsoft.AspNetCore.Mvc;

namespace firstASP.Controllers
{
    public class CourtsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
