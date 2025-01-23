using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        public WalletController(IWalletRepository walletRepository, ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }
        [HttpGet("balance")]

        public async Task<IActionResult> GetWalletBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var balance = await _walletRepository.GetWalletBalanceByUserIdAsync(userId);
            var currency = await _walletRepository.GetWalletCurrencyByUserIdAsync(userId);
            return Ok(new { balance, currency });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
