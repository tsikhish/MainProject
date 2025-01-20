using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        public TransactionsController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
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
        [HttpGet("Withdraw")]
        public IActionResult Withdraw() => View();

        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw(decimal amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var id = await _transactionRepository
                .RegisterTransactionInDepositTableAsync(userId, Status.Pending, TransactionType.Withdraw, amount);
            return RedirectToAction("TransactionHistory");
        }
        public IActionResult Deposit() => View();
        public async Task<IActionResult> MvcDeposit(decimal amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (amount <= 0) return BadRequest("Amount must be greater than 0.");
            var depositWithdrawId = await _transactionRepository
                .RegisterTransactionInDepositTableAsync(userId, Status.Pending, TransactionType.Deposit, amount);
            const string secretKey = "SecretKeyForTsikhisha";
            var hash = ComputeSHA256Hash((int)(amount * 100), userId, depositWithdrawId, secretKey);
            var transaction = new Deposit
            {
                DepositWithdrawId = depositWithdrawId,
                TransactionID = Guid.NewGuid(),
                MerchantID = userId,
                Amount = (int)(amount * 100),
                Hash = hash,
                Status=Status.Pending
            };
            TempData["Transaction"] = JsonConvert.SerializeObject(transaction);

            var response = await _transactionRepository.SendToBankingApi(transaction);
            if (response.Status == Status.Success)
            {
                return RedirectToAction("Payment", "Callback");
            }
            else
            {
                return BadRequest("Status is rejected. Amount is odd.");
            }
        }
        private string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string secretKey)
        {
            string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
