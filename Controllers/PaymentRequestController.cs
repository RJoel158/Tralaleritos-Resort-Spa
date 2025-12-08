using Microsoft.AspNetCore.Mvc;

namespace ResortTralaleritos.Controllers
{
    public class PaymentRequestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
