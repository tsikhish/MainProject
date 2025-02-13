using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.DTO;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using MvcProject.Service;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly IBankingRequestService _bankingRequestService;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly ILog _logger;
        public TransactionsController(ILog logger,IWithdrawRepository withdrawRepository, ITransactionRepository transactionRepository,
            IDepositRepository depositRepository, IBankingRequestService bankingRequestService)
        {
            _logger = logger;
            _withdrawRepository = withdrawRepository;
            _bankingRequestService = bankingRequestService;
            _transactionRepository = transactionRepository;
            _depositRepository = depositRepository;
        }

        [Authorize]
        public IActionResult TransactionHistoryPage()
        {
            return View();
        }

        public async Task<IActionResult> TransactionHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.Warn("Unauthorized access attempt to TransactionHistory.");
                return Unauthorized();
            }
            var transactions = await _transactionRepository.GetTransactionByUserId(userId);
            if (transactions == null || !transactions.Any())
            {
                _logger.Warn($"No transactions found for user with ID: {userId}");
            }
            else
            {
                _logger.Info($"Successfully retrieved {transactions.Count()} transactions for user with ID: {userId}");
            }

            return Json(new { data = transactions });
        }

        public IActionResult Deposit() => View();

        [HttpPost]
        [Route("Transactions/DepositResult")]
        public async Task<IActionResult> DepositResult([FromBody] DepositRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.Info($"Deposit request received for User ID: {userId}, Amount: {request.Amount}");

            if (request.Amount <= 0)
            {
                return Json(new { success = false, message = "Amount must be greater than zero." });
            }
            var depositId = await _depositRepository.RegisterDeposit(userId, Status.Pending, TransactionType.Deposit, request.Amount);
            _logger.Info($"Deposit registered with ID: {depositId} for User ID: {userId}");
            var response = await _bankingRequestService.SendDepositToBankingApi(depositId, request.Amount, "Deposit");
            if (response == null)
            {
                return Json(new { success = false, message = "Failed to process the transaction with the banking API." });
            }
            _logger.Info($"Deposit successfully processed. Redirecting User ID: {userId} to Payment URL: {response.PaymentUrl}");
            return Ok(new { success = true, paymentUrl = response.PaymentUrl });
        }

        public IActionResult Withdraw() => View();

        [HttpPost]
        [Route("Transactions/WithdrawRequest")]
        public async Task<IActionResult> WithdrawRequest([FromBody] DepositRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.Info($"Withdraw request received for User ID: {userId}, Amount: {request.Amount}");

            if (request.Amount <= 0)
            {
                _logger.Warn($"Withdraw failed: Invalid amount ({request.Amount}) for User ID: {userId}");
                return Json(new { success = false, message = "Amount must be greater than zero." });
            }

            await _withdrawRepository.RegisterWithdraw(userId, Status.Pending, TransactionType.Withdraw, request.Amount);
            _logger.Info($"Withdraw request successfully sent for User ID: {userId}");

            return Ok(new { Message = "Withdraw request sent successfully" });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
