using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace BankingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : Controller
    {
        //[HttpGet("Deposit")]
        //public IActionResult Deposit()
        //{
        //    return Ok("Deposit endpoint works!");
        //}
        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] Deposit deposit)
        {
            const string secretKey = "SecretKeyForTsikhisha";
            var hash = ComputeSHA256Hash((int)(deposit.Amount), deposit.MerchantID, deposit.DepositWithdrawId, secretKey);
            if (hash != deposit.Hash)
            {
                return BadRequest("Incorrect hash");
            }
            bool isAmountEven = deposit.Amount % 2 == 0;
            var status = isAmountEven ? 2 : 1;
            if (status == 2)
            {
                return Ok(new Deposit
                {
                    Status = Status.Success,
                    DepositWithdrawId=deposit.DepositWithdrawId,
                    TransactionID = deposit.TransactionID
                });
            }
            else
            {
                return Ok(
                    new
                    {
                        Status = Status.Rejected,
                    });
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
    }
}