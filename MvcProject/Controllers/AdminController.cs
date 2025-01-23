using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcProject.Models;
using MvcProject.Models.Hash;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly string _secretKey;
        private readonly IHash256 _hash;
        public AdminController(IHash256 hash, ITransactionRepository transactionRepository, IOptions<AppSettings> appSettings)
        {
            _secretKey = appSettings.Value.SecretKey;
            _hash = hash;
            _transactionRepository = transactionRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var withdraws = await _transactionRepository.GetWithdrawTransactionsForAdmins();
            return View(withdraws);
        }
        [HttpPost]
        public async Task<IActionResult> RejectWithdraw(int id)
        {
            var withdraw = await _transactionRepository.FindWithdraw(id);
            if (withdraw == null) throw new Exception("Withdraw Not Found");
            await _transactionRepository.UpdateRejectWithdraw(id);
            return Ok("Status was rejected");
        }
        [HttpPost]
        public async Task<IActionResult> AcceptWithdraw(int id)
        {
            var withdraw = await _transactionRepository.FindWithdraw(id);
            if (withdraw == null) throw new Exception("Withdraw Not Found");
            var usersFullName = await _transactionRepository.GetFullUsername(withdraw.UserId);
            var hash =_hash.ComputeSHA256Hash((int)(withdraw.Amount * 100), withdraw.UserId, withdraw.Id, usersFullName, _secretKey);
            var transaction = new Withdraw
            {
                TransactionID = id,
                MerchantID = withdraw.UserId,
                Amount = (int)(withdraw.Amount * 100),
                Hash = hash,
                UsersFullName = usersFullName
            };
            var response = await _transactionRepository.SendWithdrawToBankingApi(transaction);
            return Ok();
        }
       
    }
}
