using Microsoft.AspNetCore.Mvc;

namespace Onlink.Controllers
{
    public class AccountsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
