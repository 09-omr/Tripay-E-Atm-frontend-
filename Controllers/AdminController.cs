using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using digitalpayment3.Data;
using digitalpayment3.Models;

namespace digitalpayment3.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly AccountRepository _accountRepository;
        private readonly TransactionRepository _transactionRepository;

        public AdminController(IConfiguration configuration)
        {
            _userRepository = new UserRepository(configuration);
            _accountRepository = new AccountRepository(configuration);
            _transactionRepository = new TransactionRepository(configuration);
        }

        public IActionResult Index()
        {
            ViewBag.PageTitle = "Admin Paneli";
            
            // İstatistikler
            ViewBag.TotalUsers = _userRepository.GetTotalUserCount();
            ViewBag.TotalAccounts = _accountRepository.GetTotalAccountCount();
            ViewBag.TotalTransactions = _transactionRepository.GetTotalTransactionCount();
            
            // Son işlemler
            var recentTransactions = _transactionRepository.GetRecentTransactions(5);
            ViewBag.RecentTransactions = recentTransactions;
            
            return View();
        }

        public IActionResult Users()
        {
            ViewBag.PageTitle = "Kullanıcı Yönetimi";
            var users = _userRepository.GetAll();
            return View(users);
        }

        public IActionResult Accounts()
        {
            ViewBag.PageTitle = "Hesap Yönetimi";
            var accounts = _accountRepository.GetAll();
            return View(accounts);
        }

        public IActionResult Transactions()
        {
            ViewBag.PageTitle = "İşlem Yönetimi";
            var transactions = _transactionRepository.GetAll();
            return View(transactions);
        }

        public IActionResult Reports()
        {
            ViewBag.PageTitle = "Raporlar";
            
            // Toplam bakiyeler
            var accounts = _accountRepository.GetAll();
            ViewBag.TotalBalanceTRY = accounts.Where(a => a.Currency == "TRY").Sum(a => a.Balance);
            ViewBag.TotalBalanceUSD = accounts.Where(a => a.Currency == "USD").Sum(a => a.Balance);
            ViewBag.TotalBalanceEUR = accounts.Where(a => a.Currency == "EUR").Sum(a => a.Balance);
            ViewBag.TotalBalanceGBP = accounts.Where(a => a.Currency == "GBP").Sum(a => a.Balance);
            
            // İşlem istatistikleri
            var transactions = _transactionRepository.GetAll();
            ViewBag.TotalDeposits = transactions.Where(t => t.Type == "Deposit").Sum(t => t.Amount);
            ViewBag.TotalWithdrawals = transactions.Where(t => t.Type == "Withdraw").Sum(t => t.Amount);
            
            return View();
        }
    }
}
