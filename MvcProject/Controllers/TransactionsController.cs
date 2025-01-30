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
        public TransactionsController(IWithdrawRepository withdrawRepository,ITransactionRepository transactionRepository, 
            IDepositRepository depositRepository,IBankingRequestService bankingRequestService)
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
                if (userId == null) return Unauthorized();
                var transactions = await _transactionRepository.GetTransactionByUserId(userId);
                return Json(new { data=transactions });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                if (request.Amount <= 0)
                {
                    return Json(new { success = false, message = "Amount must be greater than zero." });
                }
                var depositId = await _depositRepository.RegisterDeposit(userId,Status.Pending,TransactionType.Deposit,request.Amount);
                var response = await _bankingRequestService.SendDepositToBankingApi(depositId,request.Amount, "Deposit");
                if (response == null)
                    return BadRequest(new { success = false, message = "Failed to process the transaction with the banking API." });
                return Ok(new { success = true, paymentUrl = response.PaymentUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        public IActionResult Withdraw() => View();

        public async Task<IActionResult> WithdrawRequest([FromBody] DepositRequestDTO request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _withdrawRepository.RegisterWithdraw
                    (userId, Status.Pending, TransactionType.Withdraw, request.Amount);
                return Ok(new { Message = "Request sent successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
