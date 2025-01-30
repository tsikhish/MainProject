using BankingApi.Helper;
using BankingApi.Models;
using BankingApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
namespace BankingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : Controller
    {
        private readonly string _secretKey;
        private readonly string _merchantId;
        private readonly string _apiUrl;
        private readonly ISendBackResponse _sendBackResponse;
        private readonly IHash _hash;
        public TransactionsController(IHash hash,IOptions<AppSettings> appSettings,ISendBackResponse sendBackResponse)
        {
            _hash=hash;
            _sendBackResponse=sendBackResponse;
            _secretKey = appSettings.Value.SecretKey;
            _merchantId = appSettings.Value.MerchantID;
            _apiUrl = appSettings.Value.ApiUrl;
        }
        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] Deposit deposit)
        {
            try
            {
                var hash = _hash.ComputeSHA256Hash((int)(deposit.Amount), _merchantId, deposit.TransactionID, _secretKey);
                if (hash != deposit.Hash)
                {
                    return BadRequest("Incorrect hash");
                }
                string paymentUrl = $"{_apiUrl}/{deposit.TransactionID}/{(int)(deposit.Amount)}";
                return Ok(new { Status = 1, PaymentUrl = paymentUrl });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("ConfirmDeposit")]
        public async Task<IActionResult> ConfirmDeposit([FromBody] Deposit deposit)
        {
            try
            {
                var hash = _hash.ComputeSHA256Hash((int)(deposit.Amount), _merchantId, deposit.TransactionID, _secretKey);
                if (hash != deposit.Hash)
                    return BadRequest("Incorrect hash");
                bool isAmountEven = (deposit.Amount / 100) % 2 == 0;
                var status = isAmountEven ? "Success" : "Rejected";
                if (status == "Success")
                    return Ok(new 
                    {
                        TransactionId = deposit.TransactionID,
                        Status = Status.Success,
                        Amount = deposit.Amount,
                    });
                else
                    return Ok(
                        new 
                        {
                            TransactionId = deposit.TransactionID,
                            Status = Status.Rejected,
                            Amount = deposit.Amount,
                        });
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("ConfirmWithdraw")]
        public async Task<IActionResult> ConfirmWithdraw([FromBody] Withdraw withdraw)
        {
            try
            {
                var hash = _hash.ComputeSHA256Hash((int)(withdraw.Amount), _merchantId, withdraw.TransactionID, withdraw.UsersFullName, _secretKey);
                if (hash != withdraw.Hash)
                    return BadRequest("Incorrect hash");
                bool isAmountEven = (withdraw.Amount / 100) % 2 == 0;
                var result = isAmountEven ? Status.Success : Status.Rejected;
                if (result == Status.Success)
                    await _sendBackResponse.SendWithdrawResultToMvcProject(withdraw, result);
                else
                    await _sendBackResponse.SendWithdrawResultToMvcProject(withdraw, result);
                return Ok(new 
                {
                    DepositWithdrawRequestId = withdraw.TransactionID,
                    Status = result,
                    Amount = withdraw.Amount,
                });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

      
    }
}