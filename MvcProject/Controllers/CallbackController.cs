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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IDepositRepository _depositRepository;
        public CallbackController(IWithdrawRepository withdrawRepository,
            ITransactionRepository transactionRepository, 
            IWalletRepository walletRepository, IDepositRepository depositRepository)
        {
            _withdrawRepository = withdrawRepository;
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
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response =await _depositRepository.SendToBankingApi(depositWithdrawRequest, "ConfirmDeposit"); 
                response.Amount = (decimal)(response.Amount / 100m);
                await _transactionRepository.RegisterTransactionInTransactionsAsync(userId,response);
                await _transactionRepository.UpdateStatus(response.DepositWithdrawRequestId, response.Status);
                if (response.Status == Status.Success)
                {
                    await _walletRepository.UpdateWalletAmount(depositWithdrawRequest);
                }
                return View(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SuccessWithdraw([FromBody] Response response)
        {
            try
            {
                response = await _withdrawRepository.GetResponse(response);   
                return View(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
