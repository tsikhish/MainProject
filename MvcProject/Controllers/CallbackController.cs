using log4net;
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
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CallbackController));  // Change to the current class
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IBankingRequestService _bankingRequestService;
        public CallbackController(IDepositRepository depositRepository, IBankingRequestService bankingRequestService,
            IWithdrawRepository withdrawRepository, ITransactionRepository transactionRepository)
        {
            _depositRepository = depositRepository;
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _transactionRepository = transactionRepository;
        }

        [Authorize]
        [Route("Callback/{DepositWithdrawId}/{Amount}")]
        public async Task<IActionResult> Index(int depositWithdrawId, int amount)
        {
            _logger.InfoFormat("Received request for transaction details. DepositWithdrawID: {0}, Amount: {1}", depositWithdrawId, amount);
            if (depositWithdrawId == 0 || amount == 0)
            {
                _logger.ErrorFormat("Invalid arguments. DepositWithdrawID or Amount cannot be zero.");
                throw new Exception("Arguments can't be null. Incorrect URL");
            }
            var transaction = await _transactionRepository.GetDepositWithdrawById(depositWithdrawId);
            if (transaction == null)
            {
                _logger.WarnFormat("Transaction with ID {0} not found.", depositWithdrawId);
                return NotFound($"Transaction with ID {depositWithdrawId} not found.");
            }
            _logger.InfoFormat("Transaction retrieved successfully. Transaction ID: {0}", depositWithdrawId);
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
                    _logger.ErrorFormat("User ID could not be retrieved from claims.");
                    return Unauthorized("User authentication failed.");
                }
                depositWithdrawRequest.UserId = userId;
                depositWithdrawRequest.TransactionType = TransactionType.Deposit;
                _logger.InfoFormat("Initiating deposit transaction for UserId: {0}, Amount: {1}", userId, depositWithdrawRequest.Amount);
                var response = await _bankingRequestService.SendDepositToBankingApi(depositWithdrawRequest, "ConfirmDeposit");
                if (response == null)
                {
                    _logger.ErrorFormat("Banking API response is null for UserId: {0}.", userId);
                    return BadRequest("Failed to process the transaction with the banking API.");
                }
                response.Amount = (decimal)(response.Amount / 100m);
                _logger.InfoFormat("Banking API response received. Adjusted Amount: {0}, TransactionId: {1}", response.Amount, response.DepositWithdrawRequestId);
                await _depositRepository.RegisterTransaction(depositWithdrawRequest, response);
                _logger.InfoFormat("Transaction successfully registered for UserId: {0}, TransactionId: {1}", userId, response.DepositWithdrawRequestId);
                return View(response);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while processing the deposit for UserId: {0}", depositWithdrawRequest.UserId), ex);
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
                    _logger.Error("Received null response in SuccessWithdraw.");
                    throw new ArgumentNullException(nameof(response), "Response cannot be null.");
                }

                _logger.InfoFormat("Processing withdrawal for TransactionId: {0}, Amount: {1}", response.DepositWithdrawRequestId, response.Amount);

                var userId = await _withdrawRepository.GetUserIdByResponce(response);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.ErrorFormat("User ID not found for TransactionId: {0}", response.DepositWithdrawRequestId);
                    return BadRequest("Invalid user ID associated with this transaction.");
                }

                response.Amount = (decimal)(response.Amount / 100m);
                _logger.InfoFormat("Adjusted withdrawal amount: {0} for TransactionId: {1}", response.Amount, response.DepositWithdrawRequestId);

                await _withdrawRepository.AddWithdrawTransactionAsync(response, userId);
                _logger.InfoFormat("Withdrawal transaction successfully registered for UserId: {0}, TransactionId: {1}", userId, response.DepositWithdrawRequestId);

                return View(response);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while processing the withdrawal for TransactionId: " + response?.DepositWithdrawRequestId, ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
