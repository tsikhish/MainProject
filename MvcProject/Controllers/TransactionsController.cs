using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcProject.Models;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Model.DTO;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using MvcProject.Models.Service;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly IBankingRequestService _bankingRequestService;
        private readonly IWithdrawRepository _withdrawRepository;
        public TransactionsController(ILogger<TransactionsController> logger, IWithdrawRepository withdrawRepository,ITransactionRepository transactionRepository, 
            IDepositRepository depositRepository,IBankingRequestService bankingRequestService)
        {
            _logger=logger;
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
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    _logger.LogWarning("Unauthorized access attempt to TransactionHistory.");
                    return Unauthorized();
                }
                var transactions = await _transactionRepository.GetTransactionByUserId(userId);
                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning("No transactions found for user: {UserId}", userId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved {TransactionCount} transactions for user: {UserId}",
                    transactions.Count(), userId);
                }
                return Json(new { data = transactions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching transaction history for user: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { message = "An error occurred while fetching transaction history. Please try again later." });
            }
        }
        public IActionResult Deposit() => View();
        [HttpPost]
        [Route("Transactions/DepositResult")]
        public async Task<IActionResult> DepositResult([FromBody] DepositRequestDTO request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Deposit request received for User ID: {UserId}, Amount: {Amount}", userId, request.Amount);
                if (request.Amount <= 0)
                {
                    _logger.LogWarning("Deposit failed: Invalid amount ({Amount}) for User ID: {UserId}", request.Amount, userId);
                    return Json(new { success = false, message = "Amount must be greater than zero." });
                }
                var depositId = await _depositRepository.RegisterDeposit(userId, Status.Pending, TransactionType.Deposit, request.Amount);
                _logger.LogInformation("Deposit registered with ID: {DepositId} for User ID: {UserId}", depositId, userId);
                var response = await _bankingRequestService.SendDepositToBankingApi(depositId, request.Amount, "Deposit");
                if (response == null)
                {
                    _logger.LogError("Failed to process deposit ID: {DepositId} for User ID: {UserId}. Banking API returned null.", depositId, userId);
                    return BadRequest(new { success = false, message = "Failed to process the transaction with the banking API." });
                }
                _logger.LogInformation("Deposit successfully processed. Redirecting User ID: {UserId} to Payment URL: {PaymentUrl}", userId, response.PaymentUrl);
                return Ok(new { success = true, paymentUrl = response.PaymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing deposit for User ID: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }
        public IActionResult Withdraw() => View();

        public async Task<IActionResult> WithdrawRequest([FromBody] DepositRequestDTO request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("Deposit request received for User ID: {UserId}, Amount: {Amount}", userId, request.Amount);
                await _withdrawRepository.RegisterWithdraw
                    (userId, Status.Pending, TransactionType.Withdraw, request.Amount);
                _logger.LogInformation("Withdraw successfully sent to Admin.");
                return Ok(new { Message = "Request sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing deposit for User ID: {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return BadRequest(new { Message = ex.Message });
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
