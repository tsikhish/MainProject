using log4net;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.DTO;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using MvcProject.Service.IService;

namespace MvcProject.Service
{
    public class WithdrawService : IWithdrawService
    {
        private readonly IWithdrawRepository _withdrawRepository;
        private readonly IBankingRequestService _bankingRequestService;
        private readonly ILog _logger; 

        public WithdrawService(IBankingRequestService bankingRequestService,
            ILoggerFactoryService loggerFactory,IWithdrawRepository withdrawRepository)
        {
            _bankingRequestService = bankingRequestService;
            _withdrawRepository = withdrawRepository;
            _logger = loggerFactory.GetLogger<WithdrawService>();

        }
        public async Task RegisterWithdraw(string userId, [FromBody] DepositRequestDTO request)
        {
            _logger.Info($"Withdraw request received for User ID: {userId}, Amount: {request.Amount}");

            await _withdrawRepository.RegisterWithdraw(userId, Status.Pending, TransactionType.Withdraw, request.Amount);
            _logger.Info($"Withdraw request successfully sent for User ID: {userId}");

        }
        public async Task SuccessWithdraw([FromBody] Response response)
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

        }
        public async Task<Response> AcceptWithdraw(int id)
        {
            var transaction = await _withdrawRepository.GetWithdrawTransaction(id);
            var response = await _bankingRequestService.SendWithdrawToBankingApi(transaction);
            return response;
        }
    }
}
