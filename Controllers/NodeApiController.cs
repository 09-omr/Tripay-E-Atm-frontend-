using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using digitalpayment3.Services;
using digitalpayment3.Models;

namespace digitalpayment3.Controllers
{
    [Authorize]
    public class NodeApiController : Controller
    {
        private readonly NodeApiService _nodeApiService;
        private readonly ILogger<NodeApiController> _logger;

        public NodeApiController(NodeApiService nodeApiService, ILogger<NodeApiController> logger)
        {
            _nodeApiService = nodeApiService;
            _logger = logger;
        }

        // Ana sayfa - API testi
        public IActionResult Index()
        {
            ViewBag.PageTitle = "Node.js API Test";
            return View();
        }

        // API Login Test
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ApiLogin()
        {
            ViewBag.PageTitle = "API Giriþ Test";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ApiLogin(string email, string password)
        {
            var result = await _nodeApiService.LoginAsync(email, password);
            
            if (result.Success && result.Data != null)
            {
                // Token'ý session'a kaydet
                HttpContext.Session.SetString("NodeApiToken", result.Data.Token);
                TempData["SuccessMessage"] = $"API'ye baþarýyla giriþ yapýldý! Token kaydedildi.";
                TempData["ApiUserInfo"] = System.Text.Json.JsonSerializer.Serialize(result.Data.User);
                return RedirectToAction("ApiAccounts");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View();
        }

        // API Register Test
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ApiRegister()
        {
            ViewBag.PageTitle = "API Kayýt Test";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ApiRegister(string email, string password, string fullName)
        {
            var result = await _nodeApiService.RegisterAsync(email, password, fullName);
            
            if (result.Success && result.Data != null)
            {
                HttpContext.Session.SetString("NodeApiToken", result.Data.Token);
                TempData["SuccessMessage"] = $"API'ye baþarýyla kayýt olundu! Token kaydedildi.";
                return RedirectToAction("ApiAccounts");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View();
        }

        // API Hesaplar
        public async Task<IActionResult> ApiAccounts()
        {
            ViewBag.PageTitle = "API Hesaplarý";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            var result = await _nodeApiService.GetMyAccountsAsync(token);
            
            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<AccountInfo>());
        }

        // API Hesap Oluþtur
        [HttpGet]
        public IActionResult ApiCreateAccount()
        {
            ViewBag.PageTitle = "API Hesap Oluþtur";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApiCreateAccount(string currency)
        {
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            var result = await _nodeApiService.CreateAccountAsync(token, currency);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{currency} hesabý baþarýyla oluþturuldu!";
                return RedirectToAction("ApiAccounts");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View();
        }

        // API Para Yatýrma
        [HttpGet]
        public async Task<IActionResult> ApiDeposit(int accountId)
        {
            ViewBag.PageTitle = "API Para Yatýr";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            var accountResult = await _nodeApiService.GetAccountByIdAsync(token, accountId);
            if (accountResult.Success && accountResult.Data != null)
            {
                ViewBag.Account = accountResult.Data;
                return View();
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("ApiAccounts");
        }

        [HttpPost]
        public async Task<IActionResult> ApiDeposit(int accountId, decimal amount, string description)
        {
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            var result = await _nodeApiService.DepositAsync(token, accountId, amount, description);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{amount} baþarýyla yatýrýldý!";
                return RedirectToAction("ApiAccounts");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction("ApiDeposit", new { accountId });
        }

        // API Para Çekme
        [HttpGet]
        public async Task<IActionResult> ApiWithdraw(int accountId)
        {
            ViewBag.PageTitle = "API Para Çek";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            var accountResult = await _nodeApiService.GetAccountByIdAsync(token, accountId);
            if (accountResult.Success && accountResult.Data != null)
            {
                ViewBag.Account = accountResult.Data;
                return View();
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("ApiAccounts");
        }

        [HttpPost]
        public async Task<IActionResult> ApiWithdraw(int accountId, decimal amount, string description)
        {
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Önce API'ye giriþ yapmalýsýnýz!";
                return RedirectToAction("ApiLogin");
            }

            var result = await _nodeApiService.WithdrawAsync(token, accountId, amount, description);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{amount} baþarýyla çekildi!";
                return RedirectToAction("ApiAccounts");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction("ApiWithdraw", new { accountId });
        }

        // API Döviz Kurlarý
        public async Task<IActionResult> ApiRates()
        {
            ViewBag.PageTitle = "API Döviz Kurlarý";
            
            var result = await _nodeApiService.GetRatesAsync("TRY");
            
            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View();
        }

        // API Logout
        public IActionResult ApiLogout()
        {
            HttpContext.Session.Remove("NodeApiToken");
            TempData["SuccessMessage"] = "API oturumu kapatýldý.";
            return RedirectToAction("Index");
        }
    }
}
