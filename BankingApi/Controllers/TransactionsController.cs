using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly string _secretKey;
        public TransactionsController(IOptions<AppSettings> appSettings)
        {
            _secretKey = appSettings.Value.SecretKey;
        }
        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] Deposit deposit)
        {
            try
            {
                var hash = ComputeSHA256Hash((int)(deposit.Amount), deposit.MerchantID, deposit.TransactionID, _secretKey);
                if (hash != deposit.Hash)
                {
                    return BadRequest("Incorrect hash");
                }
                string paymentUrl = $"https://localhost:7116/Callback/{deposit.TransactionID}/{(int)(deposit.Amount)}";
                return Ok(new { Status = 1, PaymentUrl = paymentUrl });
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost("ConfirmDeposit")]
        public async Task<IActionResult> ConfirmDeposit([FromBody] Deposit deposit)
        {
            try
            {
                var hash = ComputeSHA256Hash((int)(deposit.Amount), deposit.MerchantID, deposit.TransactionID, _secretKey);
                if (hash != deposit.Hash)
                    return BadRequest("Incorrect hash");
                bool isAmountEven = (deposit.Amount / 100) % 2 == 0;
                var status = isAmountEven ? "Success" : "Rejected";
                if (status == "Success")
                    return Ok(new Response
                    {
                        Status = Status.Success,
                        Amount = deposit.Amount,
                        DepositWithdrawRequestId = deposit.TransactionID,
                    });
                else
                    return Ok(
                        new Response
                        {
                            Status = Status.Rejected,
                            Amount = deposit.Amount,
                            DepositWithdrawRequestId = deposit.TransactionID
                        });
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost("ConfirmWithdraw")]
        public async Task<IActionResult> ConfirmWithdraw([FromBody] Withdraw withdraw)
        {
            try
            {
                var hash = ComputeSHA256Hash((int)(withdraw.Amount), withdraw.MerchantID, withdraw.TransactionID, withdraw.UsersFullName, _secretKey);
                if (hash != withdraw.Hash)
                    return BadRequest("Incorrect hash");
                bool isAmountEven = withdraw.Amount / 100 % 2 == 0;
                var status = isAmountEven ? "Success" : "Rejected";
                var result = isAmountEven ? Status.Success : Status.Rejected;
                if (status == "Success")
                    await SendResultToMvcProject(withdraw, Status.Success);
                else
                    await SendResultToMvcProject(withdraw, Status.Rejected);
                return Ok(new Response
                {
                    Status = result,
                    Amount = withdraw.Amount,
                    DepositWithdrawRequestId = withdraw.TransactionID,
                });
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private async Task SendResultToMvcProject(Withdraw withdraw, Status status)
        {
            try
            {
                using var client = new HttpClient();
                var request = new Response
                {
                    DepositWithdrawRequestId = withdraw.TransactionID,
                    Amount = withdraw.Amount,
                    Status = status
                };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://localhost:7116/Callback/SuccessWithdraw", content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to notify MVC project about the transaction result.");
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string userFullName,string secretKey)
        {
            string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{userFullName}+{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
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