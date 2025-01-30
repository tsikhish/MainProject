using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Security.Claims;
namespace MvcProject.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger _logger;
        public WalletController(ILogger<WalletController> logger,IWalletRepository walletRepository)
        {
            _logger = logger;
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
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims.");
                    return Unauthorized(new { success = false, message = "User not authenticated." });
                }
                _logger.LogInformation("Fetching wallet balance for user: {UserId}", userId);
                var balance = await _walletRepository.GetWalletBalanceByUserIdAsync(userId);
                var currency = await _walletRepository.GetWalletCurrencyByUserIdAsync(userId);
                if (balance == 0 || currency == 0)
                {
                    _logger.LogWarning("Wallet data not found for user: {UserId}", userId);
                    return NotFound(new { success = false, message = "Wallet balance or currency not found." });
                }
                var currencySymbol = GetCurrencySymbol(currency);
                _logger.LogInformation("Wallet balance retrieved for user {UserId}: {Balance} {Currency}", userId, balance, currencySymbol);
                return Ok(new { success = true, balance, currency = currencySymbol });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching wallet balance for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { success = false, message = "Internal server error. Please try again later." });
            }
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
