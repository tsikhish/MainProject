using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.DTO;
using MvcProject.Service.IService;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IWithdrawService _withdrawService;
        private readonly IDepositService _depositService;
        public TransactionsController(IDepositService depositService,ITransactionService transactionService,IWithdrawService withdrawService)
        {
            _depositService = depositService;
            _transactionService = transactionService;
            _withdrawService = withdrawService;
        }

        [Authorize]
        public IActionResult TransactionHistoryPage()
        {
            return View();
        }

        public async Task<IActionResult> TransactionHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transactions = await _transactionService.TransactionHistory(userId);
            return Json(new { data = transactions });
        }

        public IActionResult Deposit() => View();

        [HttpPost]
        [Route("Transactions/DepositResult")]
        public async Task<IActionResult> DepositResult([FromBody] DepositRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _depositService.Deposit(userId,request);
            return Ok(new { success = true, paymentUrl = response.PaymentUrl });
        }

        public IActionResult Withdraw() => View();

        [HttpPost]
        [Route("Transactions/WithdrawRequest")]
        public async Task<IActionResult> WithdrawRequest([FromBody] DepositRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _withdrawService.RegisterWithdraw(userId, request);
            return Ok(new { Message = "Withdraw request sent successfully" });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
