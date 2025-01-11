using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        public WalletController(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }
        [HttpGet("balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var balance = _walletRepository.GetWalletBalanceByUserIdAsync(userId);
            var currency = _walletRepository.GetWalletCurrencyByUserIdAsync(userId);
            return Ok(new { balance, currency });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
