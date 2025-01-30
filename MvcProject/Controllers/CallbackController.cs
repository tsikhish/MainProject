using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using MvcProject.Models.Service;
using System.Security.Claims;
using System.Transactions;

namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly IDepositRepository _depositRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IBankingRequestService _bankingRequestService;
        public CallbackController(IDepositRepository depositRepository,IBankingRequestService bankingRequestService,IWithdrawRepository withdrawRepository,
            ITransactionRepository transactionRepository)
        {
            _depositRepository = depositRepository;
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _transactionRepository = transactionRepository;
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
                depositWithdrawRequest.UserId = userId; 
                depositWithdrawRequest.TransactionType = TransactionType.Deposit;
                var response =await _bankingRequestService.SendDepositToBankingApi(depositWithdrawRequest, "ConfirmDeposit"); 
                response.Amount = (decimal)(response.Amount / 100m);
                await _depositRepository.RegisterTransaction(depositWithdrawRequest,response);
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
                if (response == null)
                {
                    throw new ArgumentNullException(nameof(response), "Response cannot be null.");
                }
                var userId = await _withdrawRepository.GetUserIdByResponce(response);
                response.Amount = (decimal)(response.Amount / 100m);
                await _withdrawRepository.AddWithdrawTransactionAsync(response, userId);
                return View(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
