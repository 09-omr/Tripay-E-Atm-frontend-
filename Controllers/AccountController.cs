using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using digitalpayment3.Services;
using digitalpayment3.Data;
using digitalpayment3.Models;
using System.Security.Claims;

namespace digitalpayment3.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly NodeApiService _nodeApiService;
        private readonly AccountRepository _accountRepository;

        public AccountController(NodeApiService nodeApiService, IConfiguration configuration)
        {
            _nodeApiService = nodeApiService;
            _accountRepository = new AccountRepository(configuration);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PageTitle = "Hesaplarým";
            
            // PostgreSQL'den hesaplarý çek
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var accounts = _accountRepository.GetByUserId(userId);
                return View(accounts);
            }

            return View(new List<AccountViewModel>());
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewBag.PageTitle = "Hesap Detayý";
            
            var account = _accountRepository.GetById(id);
            
            if (account != null)
            {
                // Döviz kurlarýný Node.js API'den çek
                try
                {
                    var ratesResult = await _nodeApiService.GetRatesAsync(account.Currency);
                    if (ratesResult.Success && ratesResult.Data != null)
                    {
                        ViewBag.ExchangeRates = ratesResult.Data;
                    }
                }
                catch (Exception ex)
                {
                    // Hata olursa boþ býrak
                    ViewBag.ExchangeRates = null;
                }
                
                return View(account);
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PageTitle = "Yeni Hesap Oluþtur";
            return View();
        }

        [HttpPost]
        public IActionResult Create(AccountViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                model.OwnerName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Kullanýcý";
                model.Balance = 0;
                model.IsActive = true;
                
                _accountRepository.Insert(model, userId);
                
                TempData["SuccessMessage"] = "Hesap baþarýyla oluþturuldu!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Hesap oluþturulamadý!";
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.PageTitle = "Hesap Düzenle";
            
            var account = _accountRepository.GetById(id);
            
            if (account != null)
            {
                return View(account);
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                _accountRepository.Update(model);
                TempData["SuccessMessage"] = "Hesap baþarýyla güncellendi!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Hesap güncellenemedi!";
            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewBag.PageTitle = "Hesap Sil";
            
            var account = _accountRepository.GetById(id);
            
            if (account != null)
            {
                return View(account);
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var account = _accountRepository.GetById(id);
            
            if (account != null)
            {
                if (account.Balance > 0)
                {
                    TempData["ErrorMessage"] = "Bakiyesi olan hesap silinemez!";
                    return RedirectToAction("Delete", new { id });
                }

                _accountRepository.Delete(id);
                TempData["SuccessMessage"] = "Hesap baþarýyla silindi!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Hesap bulunamadý!";
            return RedirectToAction("Index");
        }
    }
}
