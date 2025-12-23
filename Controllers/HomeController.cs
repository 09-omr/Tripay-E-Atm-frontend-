using Microsoft.AspNetCore.Mvc;
using digitalpayment3.Models;

namespace digitalpayment3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.PageTitle = "Ana Sayfa";
            return View();
        }

        public IActionResult About()
        {
            ViewBag.PageTitle = "Hakkýmýzda";
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewBag.PageTitle = "Ýletiþim";
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["ContactName"] = model.FullName;
                TempData["ContactEmail"] = model.Email;
                TempData["ContactMessage"] = model.Message;
                TempData["SuccessMessage"] = "Mesajýnýz baþarýyla gönderildi!";
                return RedirectToAction("ThankYou");
            }
            return View(model);
        }

        public IActionResult ThankYou()
        {
            ViewBag.PageTitle = "Teþekkürler";
            ViewBag.ContactName = TempData["ContactName"];
            ViewBag.ContactEmail = TempData["ContactEmail"];
            ViewBag.ContactMessage = TempData["ContactMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
