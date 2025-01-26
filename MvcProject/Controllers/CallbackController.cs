using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MvcProject.Models;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly string _secretKey;
        private readonly IHash256 _hash;
        private readonly IWalletRepository _walletRepository;
        private readonly IDepositRepository _depositRepository;
        public CallbackController(IOptions<AppSettings> appSettings, IHash256 hash,
            ITransactionRepository transactionRepository, 
            IWalletRepository walletRepository, IDepositRepository depositRepository)
        {
            _secretKey = appSettings.Value.SecretKey;
            _hash = hash;
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _depositRepository = depositRepository;
        }
        [Authorize]
        [Route("Callback/{DepositWithdrawId}/{Amount}")]
        public async Task<IActionResult> Index(int depositWithdrawId,int amount)
        {
            if (depositWithdrawId == 0 || amount == 0) throw new Exception("Arguments cant be null. Incorrect URL");
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
                deposit.Hash = hash;
                var response =await _depositRepository.SendToBankingApi(deposit, "ConfirmDeposit"); 
                deposit.Amount = (decimal)(deposit.Amount / 100m);
                response.Amount = (decimal)(response.Amount / 100m);
                await _transactionRepository.RegisterTransactionInTransactionsAsync(deposit.MerchantID,response);
                await _transactionRepository.UpdateStatus(deposit.TransactionID, response.Status);
                if (response.Status == Status.Success)
                {
                    await _walletRepository.UpdateWalletAmount(deposit);
                }
                return View(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SuccessWithdraw([FromBody] Response response)
        {
            try
            {
                if (response == null || !ModelState.IsValid)
                {
                    return BadRequest("Invalid transaction data.");
                }

                if (!Enum.IsDefined(typeof(Status), response.Status))
                {
                    return BadRequest("Invalid transaction status.");
                }
                var userId = await _depositRepository.GetUserIdByResponse(response);
                response.Amount = (decimal)(response.Amount / 100m);
                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _transactionRepository.RegisterTransactionInTransactionsAsync(userId, response);
                    await _transactionRepository.UpdateStatus(response.DepositWithdrawRequestId, response.Status);

                    if (response.Status == Status.Success)
                    {
                        await _walletRepository.UpdateWalletAmount(userId, response);
                    }

                    transaction.Complete();
                }
                return View(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
