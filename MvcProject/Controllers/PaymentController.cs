using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using MvcProject.Service;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ILog _logger;
        private readonly IBankingRequestService _bankingRequestService;
        private readonly ITransactionRepository _transactionRepository;

        public PaymentController(ILog logger, IBankingRequestService bankingRequestService,
         ITransactionRepository transactionRepository)
        {
            _logger = logger;
            _bankingRequestService = bankingRequestService;
            _transactionRepository = transactionRepository;
        }
       
        [Authorize]
        [Route("Payment/{DepositWithdrawId}/{Amount}")]
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
        public async Task<IActionResult> SendingDeposit(DepositWithdrawRequest depositWithdrawRequest)
        {
            depositWithdrawRequest.TransactionType = TransactionType.Deposit;
            _logger.InfoFormat("Initiating deposit transaction for UserId: {0}, Amount: {1}", depositWithdrawRequest.UserId, depositWithdrawRequest.Amount);
            var response = await _bankingRequestService.SendDepositToBankingApi(depositWithdrawRequest, "ConfirmDeposit");
            if (response == null)
            {
                _logger.ErrorFormat("Banking API response is null for UserId: {0}.", depositWithdrawRequest.UserId);
                return BadRequest("Failed to process the transaction with the banking API.");
            }
            return View(response);
        }
    }
}
