using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using digitalpayment3.Services;

namespace digitalpayment3.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly NodeApiService _nodeApiService;

        public TransactionController(NodeApiService nodeApiService)
        {
            _nodeApiService = nodeApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Deposit(int accountId)
        {
            ViewBag.PageTitle = "Para Yatýr";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturum süresi dolmuþ. Lütfen tekrar giriþ yapýn.";
                return RedirectToAction("Login", "Auth");
            }

            var accountResult = await _nodeApiService.GetAccountByIdAsync(token, accountId);
            if (accountResult.Success && accountResult.Data != null)
            {
                ViewBag.Account = accountResult.Data;
                return View();
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(int accountId, decimal amount, string description)
        {
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturum süresi dolmuþ. Lütfen tekrar giriþ yapýn.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _nodeApiService.DepositAsync(token, accountId, amount, description);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{amount} baþarýyla yatýrýldý!";
                return RedirectToAction("Index", "Account");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction("Deposit", new { accountId });
        }

        [HttpGet]
        public async Task<IActionResult> Withdraw(int accountId)
        {
            ViewBag.PageTitle = "Para Çek";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturum süresi dolmuþ. Lütfen tekrar giriþ yapýn.";
                return RedirectToAction("Login", "Auth");
            }

            var accountResult = await _nodeApiService.GetAccountByIdAsync(token, accountId);
            if (accountResult.Success && accountResult.Data != null)
            {
                ViewBag.Account = accountResult.Data;
                return View();
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("Index", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int accountId, decimal amount, string description)
        {
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturum süresi dolmuþ. Lütfen tekrar giriþ yapýn.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _nodeApiService.WithdrawAsync(token, accountId, amount, description);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"{amount} baþarýyla çekildi!";
                return RedirectToAction("Index", "Account");
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction("Withdraw", new { accountId });
        }

        public async Task<IActionResult> Summary()
        {
            ViewBag.PageTitle = "Hesap Özeti";
            
            var token = HttpContext.Session.GetString("NodeApiToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturum süresi dolmuþ. Lütfen tekrar giriþ yapýn.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _nodeApiService.GetMyAccountsAsync(token);
            
            if (result.Success && result.Data != null)
            {
                return View(result.Data);
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
            return View(new List<AccountInfo>());
        }
    }
}
