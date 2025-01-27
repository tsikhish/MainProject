using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Security.Claims;
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
                    return Unauthorized(new { success = false, message = "User not authenticated." });
                var balance = await _walletRepository.GetWalletBalanceByUserIdAsync(userId);
                var currency = await _walletRepository.GetWalletCurrencyByUserIdAsync(userId);

                if (balance == null || currency == null)
                    return NotFound(new { success = false, message = "Wallet balance or currency not found." });
                var currencySymbol = GetCurrencySymbol(currency);

                return Ok(new { success = true, balance, currency = currencySymbol });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
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
