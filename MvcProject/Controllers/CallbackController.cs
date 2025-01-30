using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using MvcProject.Models.Service;
using System.Security.Claims;
using System.Transactions;

namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IBankingRequestService _bankingRequestService;
        public CallbackController(ILogger<CallbackController> logger,IDepositRepository depositRepository,IBankingRequestService bankingRequestService,IWithdrawRepository withdrawRepository,
            ITransactionRepository transactionRepository)
        {
            _logger = logger;
            _depositRepository = depositRepository;
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _transactionRepository = transactionRepository;
        }
        [Authorize]
        [Route("Callback/{DepositWithdrawId}/{Amount}")]
        public async Task<IActionResult> Index(int depositWithdrawId,int amount)
        {
            _logger.LogInformation("Received request for transaction details. DepositWithdrawID: {DepositWithdrawId}, Amount: {Amount}", depositWithdrawId, amount);
            if (depositWithdrawId == 0 || amount == 0)
            {
                _logger.LogError("Invalid arguments. DepositWithdrawID or Amount cannot be zero.");
                throw new Exception("Arguments can't be null. Incorrect URL");
            }
            var transaction = await _transactionRepository.GetDepositWithdrawById(depositWithdrawId);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {DepositWithdrawId} not found.", depositWithdrawId);
                return NotFound($"Transaction with ID {depositWithdrawId} not found.");
            }
            _logger.LogInformation("Transaction retrieved successfully. Transaction ID: {DepositWithdrawId}", depositWithdrawId);
            return View("Index", transaction);
        }

        [HttpPost]
        public async Task<IActionResult> SuccessDeposit(DepositWithdrawRequest depositWithdrawRequest)
        {

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID could not be retrieved from claims.");
                    return Unauthorized("User authentication failed.");
                }
                depositWithdrawRequest.UserId = userId;
                depositWithdrawRequest.TransactionType = TransactionType.Deposit;
                _logger.LogInformation("Initiating deposit transaction for UserId: {UserId}, Amount: {Amount}", userId, depositWithdrawRequest.Amount);
                var response = await _bankingRequestService.SendDepositToBankingApi(depositWithdrawRequest, "ConfirmDeposit");
                if (response == null)
                {
                    _logger.LogError("Banking API response is null for UserId: {UserId}.", userId);
                    return BadRequest("Failed to process the transaction with the banking API.");
                }
                response.Amount = (decimal)(response.Amount / 100m);
                _logger.LogInformation("Banking API response received. Adjusted Amount: {Amount}, TransactionId: {TransactionId}", response.Amount, response.DepositWithdrawRequestId);
                await _depositRepository.RegisterTransaction(depositWithdrawRequest, response);
                _logger.LogInformation("Transaction successfully registered for UserId: {UserId}, TransactionId: {TransactionId}", userId, response.DepositWithdrawRequestId);
                return View(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the deposit for UserId: {UserId}", depositWithdrawRequest.UserId);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SuccessWithdraw([FromBody] Response response)
        {
            try
            {
                if (response == null)
                {
                    _logger.LogError("Received null response in SuccessWithdraw.");
                    throw new ArgumentNullException(nameof(response), "Response cannot be null.");
                }
                _logger.LogInformation("Processing withdrawal for TransactionId: {TransactionId}, Amount: {Amount}", response.DepositWithdrawRequestId, response.Amount);
                var userId = await _withdrawRepository.GetUserIdByResponce(response);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID not found for TransactionId: {TransactionId}", response.DepositWithdrawRequestId);
                    return BadRequest("Invalid user ID associated with this transaction.");
                }
                response.Amount = (decimal)(response.Amount / 100m);
                _logger.LogInformation("Adjusted withdrawal amount: {Amount} for TransactionId: {TransactionId}", response.Amount, response.DepositWithdrawRequestId);
                await _withdrawRepository.AddWithdrawTransactionAsync(response, userId);
                _logger.LogInformation("Withdrawal transaction successfully registered for UserId: {UserId}, TransactionId: {TransactionId}", userId, response.DepositWithdrawRequestId);
                return View(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the withdrawal for TransactionId: {TransactionId}", response?.DepositWithdrawRequestId);
                return BadRequest(ex.Message);
            }
        }
    }
}
