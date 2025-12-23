using Microsoft.AspNetCore.Mvc;
using digitalpayment3.Data;
using digitalpayment3.Models;
using System.Security.Claims;

namespace digitalpayment3.ViewComponents
{
    public class RecentTransactionsViewComponent : ViewComponent
    {
        private readonly TransactionRepository _transactionRepository;

        public RecentTransactionsViewComponent(IConfiguration configuration)
        {
            _transactionRepository = new TransactionRepository(configuration);
        }

        public IViewComponentResult Invoke(int count = 5)
        {
            // HttpContext üzerinden kullanýcý bilgisini al
            var userIdClaim = UserClaimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                // Kullanýcý giriþ yapmamýþsa boþ liste döndür
                return View(new List<TransactionViewModel>());
            }

            // Son N iþlemi getir
            var transactions = _transactionRepository.GetRecentByUserId(userId, count);
            return View(transactions);
        }
    }
}
