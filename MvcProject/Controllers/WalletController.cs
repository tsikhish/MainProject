using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Security.Claims;
namespace MvcProject.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        public WalletController(IWalletRepository walletRepository)
        {
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
                var balance = await _walletRepository.GetWalletBalanceByUserIdAsync(userId);
                var currency = await _walletRepository.GetWalletCurrencyByUserIdAsync(userId);
                if (balance == 0 || currency == 0)
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
