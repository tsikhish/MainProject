using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcProject.Models;
using MvcProject.Models.Hash;
using MvcProject.Models.IRepository;
using MvcProject.Models.Model;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly string _secretKey;
        private readonly IHash256 _hash;
        public CallbackController(IOptions<AppSettings> appSettings, IHash256 hash, ITransactionRepository transactionRepository)
        {
            _secretKey= appSettings.Value.SecretKey;
            _hash= hash;
            _transactionRepository = transactionRepository;
        }
        [Route("Callback/{DepositWithdrawId}/{Amount}")]
        public async Task<IActionResult> Index(int depositWithdrawId,int amount)
        {
            var transaction =await _transactionRepository.GetDepositWithdrawById(depositWithdrawId);
            if (transaction == null)
            {
                return NotFound($"Transaction with ID {depositWithdrawId} not found.");
            }
            return View("Index",transaction);
        }

        [HttpPost]
        public async Task<IActionResult> SuccessDeposit(DepositWithdrawRequest depositWithdrawRequest)
        {
            try
            {
                var deposit = new Deposit
                {
                    Amount = (int)(depositWithdrawRequest.Amount * 100),
                    MerchantID = depositWithdrawRequest.UserId,
                    TransactionID = depositWithdrawRequest.Id,
                };
                var hash = _hash.ComputeSHA256Hash((int)(deposit.Amount), deposit.MerchantID, deposit.TransactionID, _secretKey);
                deposit.Hash=hash;
                var response = _transactionRepository.SendToBankingApi(deposit, "ConfirmDeposit"); 
                deposit.Amount = (decimal)(deposit.Amount / 100);
                await _transactionRepository.RegisterSuccessTransactionInTransactionsAsync(deposit);
                await _transactionRepository.UpdateWalletAmount(deposit);
                await _transactionRepository.UpdateSuccessDepositTable(deposit);
                return View();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SuccessWithdraw([FromBody]Response response) {
            try
            {
                var userId = await _transactionRepository.GetUserIdByResponse(response);
                response.Amount = (decimal)(response.Amount / 100);
                await _transactionRepository.RegisterRejectedTransactionInTransactionsAsync(userId, response);
                await _transactionRepository.UpdateWalletAmount(userId, response);
                await _transactionRepository.UpdateSuccessWithdrawTable(response);
                return Ok("response sucessfully accepted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
