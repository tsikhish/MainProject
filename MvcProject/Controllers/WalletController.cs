using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        public WalletController(IWalletRepository walletRepository, ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }
        [HttpGet("balance")]

        public async Task<IActionResult> GetWalletBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var balance = await _walletRepository.GetWalletBalanceByUserIdAsync(userId);
            var currency = await _walletRepository.GetWalletCurrencyByUserIdAsync(userId);
            return Ok(new { balance, currency });
        }
       
        
        [HttpGet("AdminDashboard")]
        [Authorize("Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var transactions = await _transactionRepository.GetWithdrawTransactionsForAdmins();
            return View(transactions);
        }
        //[HttpGet("Accept")]
        //[Authorize("Admin")]
        //public async Task<IActionResult> Accept(DepositWithdrawRequest transaction)
        //{
        //    try
        //    {
        //        if (transaction.TransactionType != TransactionType.Withdraw && transaction.Status != Status.Pending)
        //        {
        //            throw new Exception("You cant send request");
        //        }
        //       // await _walletRepository.WalletWithdraw(transaction.UserId, transaction.Amount);
        //        await _transactionRepository.SendToBankingApi(transaction);
        //        return RedirectToAction("TransactionHistory");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [HttpGet("Reject")]
        [Authorize("Admin")]
        public async Task<IActionResult> Reject(DepositWithdrawRequest transaction)
        {
            if (transaction.TransactionType == TransactionType.Withdraw && transaction.Status == Status.Pending)
            {
                transaction.Status = Status.Rejected;
                await _transactionRepository.UpdateTransactionAsync(transaction);
            }
            return RedirectToAction("TransactionHistory");
        }
        //private string ComputeSHA256Hash(decimal amount, string merchantId, int transactionId, string secretKey)
        //{
        //    string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{secretKey}";
        //    using (var sha256 = SHA256.Create())
        //    {
        //        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
        //        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        //    }
        //}
        public IActionResult Index()
        {
            return View();
        }
    }
}
