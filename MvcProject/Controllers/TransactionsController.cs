using Azure;
using log4net;
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
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly IBankingRequestService _bankingRequestService;
        private readonly IWithdrawRepository _withdrawRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TransactionsController));

        public TransactionsController(IWithdrawRepository withdrawRepository, ITransactionRepository transactionRepository,
            IDepositRepository depositRepository, IBankingRequestService bankingRequestService)
        {
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
            catch (Exception ex)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.Error($"An error occurred while fetching transaction history for user with ID: {userId}", ex);
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
                _logger.Info($"Deposit request received for User ID: {userId}, Amount: {request.Amount}");

                if (request.Amount <= 0)
                {
                    _logger.Warn($"Deposit failed: Invalid amount ({request.Amount}) for User ID: {userId}");
                    return Json(new { success = false, message = "Amount must be greater than zero." });
                }

                var depositId = await _depositRepository.RegisterDeposit(userId, Status.Pending, TransactionType.Deposit, request.Amount);
                _logger.Info($"Deposit registered with ID: {depositId} for User ID: {userId}");

                var response = await _bankingRequestService.SendDepositToBankingApi(depositId, request.Amount, "Deposit");
                if (response == null)
                {
                    _logger.Error($"Failed to process deposit ID: {depositId} for User ID: {userId}. Banking API returned null.");
                    return BadRequest(new { success = false, message = "Failed to process the transaction with the banking API." });
                }

                _logger.Info($"Deposit successfully processed. Redirecting User ID: {userId} to Payment URL: {response.PaymentUrl}");
                return Ok(new { success = true, paymentUrl = response.PaymentUrl });
            }
            catch (Exception ex)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.Error($"An error occurred while processing deposit for User ID: {userId}", ex);
                return StatusCode(500, new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        public IActionResult Withdraw() => View();

        [HttpPost]
        [Route("Transactions/WithdrawRequest")]
        public async Task<IActionResult> WithdrawRequest([FromBody] DepositRequestDTO request)
        {
            try
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
            catch (Exception ex)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.Error($"An error occurred while processing withdraw for User ID: {userId}", ex);
                return BadRequest(new { Message = ex.Message });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
