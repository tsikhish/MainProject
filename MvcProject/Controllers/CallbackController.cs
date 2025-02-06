using log4net;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
namespace MvcProject.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ILog _logger;
        private readonly IDepositRepository _depositRepository;
        private readonly IWithdrawRepository _withdrawRepository;
        public CallbackController(ILog logger,IDepositRepository depositRepository,
            IWithdrawRepository withdrawRepository)
        {
            _depositRepository = depositRepository;
            _withdrawRepository = withdrawRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SuccessDeposit([FromBody] Response response)
        {
            try
            {
                if (response == null)
                {
                    _logger.Error("Received null response in SuccessWithdraw.");
                    throw new ArgumentNullException(nameof(response), "Response cannot be null.");
                }
                var userId = await _withdrawRepository.GetUserIdByResponce(response);
                _logger.InfoFormat("Processing withdrawal for TransactionId: {0}, Amount: {1}", response.DepositWithdrawRequestId, response.Amount);
                response.Amount = (decimal)(response.Amount / 100m);
                _logger.InfoFormat("Banking API response received. Adjusted Amount: {0}, TransactionId: {1}", response.Amount, response.DepositWithdrawRequestId);
                await _depositRepository.RegisterTransaction(userId, response);
                _logger.InfoFormat("Transaction successfully registered for UserId: {0}, TransactionId: {1}", userId, response.DepositWithdrawRequestId);
                return Ok("Response was successfull");
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("An error occurred while processing the deposit"), ex);
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
                    _logger.Error("Received null response in SuccessWithdraw.");
                    throw new ArgumentNullException(nameof(response), "Response cannot be null.");
                }
                _logger.InfoFormat("Processing withdrawal for TransactionId: {0}, Amount: {1}", response.DepositWithdrawRequestId, response.Amount);
                var userId = await _withdrawRepository.GetUserIdByResponce(response);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.ErrorFormat("User ID not found for TransactionId: {0}", response.DepositWithdrawRequestId);
                    return BadRequest("Invalid user ID associated with this transaction.");
                }
                response.Amount = (decimal)(response.Amount / 100m);
                _logger.InfoFormat("Adjusted withdrawal amount: {0} for TransactionId: {1}", response.Amount, response.DepositWithdrawRequestId);
                await _withdrawRepository.AddWithdrawTransactionAsync(response, userId);
                _logger.InfoFormat("Withdrawal transaction successfully registered for UserId: {0}, TransactionId: {1}", userId, response.DepositWithdrawRequestId);
                return View(response);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while processing the withdrawal for TransactionId: " + response?.DepositWithdrawRequestId, ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
