using log4net;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using System.Security.Claims;
namespace MvcProject.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ILog _logger;
        public WalletController(ILog logger,IWalletRepository walletRepository)
        {
            _logger=logger;
            _walletRepository = walletRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("Wallet/GetWalletBalance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.Warn("User ID not found in claims.");
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }
            _logger.Info("Fetching wallet balance for user: " + userId);
            var balance = await _walletRepository.GetWalletBalanceByUserIdAsync(userId);
            var currency = await _walletRepository.GetWalletCurrencyByUserIdAsync(userId);

            if (balance == null || currency == null)
            {
                _logger.Warn("Wallet data not found for user: " + userId);
                return NotFound(new { success = false, message = "Wallet balance or currency not found." });
            }
            var currencySymbol = GetCurrencySymbol(currency);
            _logger.Info("Wallet balance retrieved for user " + userId + ": " + balance + " " + currencySymbol);
            return Ok(new { success = true, balance, currency = currencySymbol });
        }
        private string GetCurrencySymbol(int currencyId)
        {
            return currencyId switch
            {
                (int)Currency.EUR => "€",
                (int)Currency.USD => "$",
                (int)Currency.GEL => "₾",
                _ => string.Empty 
            };
        }


    }
}
