using log4net;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Repository.IRepository;
namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ILog _logger;
        private readonly IDepositRepository _depositRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        public CallbackController(ILog logger, IDepositRepository depositRepository,
            IWithdrawRepository withdrawRepository)
        {
            _depositRepository = depositRepository;
            _withdrawRepository = withdrawRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SuccessDeposit([FromBody] Response response)
        {
            if (response == null)
            {
                _logger.Error("Received null response in SuccessWithdraw.");
                throw new ArgumentNullException(nameof(response), "Response cannot be null.");
            }
            response.Amount = (decimal)(response.Amount / 100m);
            _logger.InfoFormat("Banking API response received. Adjusted Amount: {0}, TransactionId: {1}", response.Amount, response.DepositWithdrawRequestId);
            await _depositRepository.RegisterTransaction(response);
            _logger.InfoFormat("Transaction successfully registered for TransactionId: {1}", response.DepositWithdrawRequestId);
            return Ok("Response was successfull");
        }

        [HttpPost]
        public async Task<IActionResult> SuccessWithdraw([FromBody] Response response)
        {
            if (response == null)
            {
                _logger.Error("Received null response in SuccessWithdraw.");
                throw new ArgumentNullException(nameof(response), "Response cannot be null.");
            }
            response.Amount = (decimal)(response.Amount / 100m);
            _logger.InfoFormat("Adjusted withdrawal amount: {0} for TransactionId: {1}", response.Amount, response.DepositWithdrawRequestId);
            await _withdrawRepository.AddWithdrawTransactionAsync(response);
            _logger.InfoFormat("Withdrawal transaction successfully registered for TransactionId: {1}", response.DepositWithdrawRequestId);
            return View(response);
        }
    }
}