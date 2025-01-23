using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcProject.Models;
using MvcProject.Models.Hash;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly string _secretKey;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHash256 _hash;
        public TransactionsController(IOptions<AppSettings> appSettings,
            ITransactionRepository transactionRepository,IHash256 hash)
        {
            _secretKey = appSettings.Value.SecretKey;
            _transactionRepository = transactionRepository;
            _hash = hash;
        }
        public async Task<IActionResult> TransactionHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();
                var transaction = await _transactionRepository.GetTransactionByUserId(userId);
                if (transaction == null || !transaction.Any())
                {
                    ViewBag.Message = "No transactions found.";
                }
                return View(transaction);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        public IActionResult Deposit() => View();
        public async Task<IActionResult> DepositResult(decimal amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var depositWithdrawId = await _transactionRepository
                .RegisterTransactionInDepositTableAsync(userId, Status.Pending, TransactionType.Deposit, amount);
            var hash = _hash.ComputeSHA256Hash((int)(amount * 100), userId, depositWithdrawId, _secretKey);
            var transaction = new Deposit
            {
                TransactionID = depositWithdrawId,
                MerchantID = userId,
                Amount = (int)(amount * 100),
                Hash = hash,
            };
            var response = await _transactionRepository.SendToBankingApi(transaction, "Deposit");
            return View("DepositResult", response.PaymentUrl);
        }
        public IActionResult Withdraw() => View();
        public async Task<IActionResult> WithdrawRequest(decimal amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var depositWithdrawId = await _transactionRepository
                .RegisterTransactionInDepositTableAsync(userId, Status.Pending, TransactionType.Withdraw, amount);
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
