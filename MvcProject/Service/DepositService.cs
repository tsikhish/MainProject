using log4net;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Exceptions;
using MvcProject.Models;
using MvcProject.Models.DTO;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using MvcProject.Service.IService;

namespace MvcProject.Service
{
    public class DepositService : IDepositService
    {
        private readonly ILog _logger;
        private readonly IDepositRepository _depositRepository;
        private readonly IBankingRequestService _bankingRequestService;
        public DepositService(ILoggerFactoryService loggerFactory,
            IDepositRepository depositRepository, IBankingRequestService bankingRequestService)
        {
            _bankingRequestService = bankingRequestService;
            _logger = loggerFactory.GetLogger<TransactionService>();
            _depositRepository = depositRepository;
        }
        public async Task<Response> Deposit(string userId, [FromBody] DepositRequestDTO request)
        {
            try
            {
                _logger.Info($"Deposit request received for User ID: {userId}, Amount: {request.Amount}");

                var depositId = await _depositRepository.RegisterDeposit(userId, Status.Pending, TransactionType.Deposit, request.Amount);
                _logger.Info($"Deposit registered with ID: {depositId} for User ID: {userId}");
                var response = await _bankingRequestService.SendDepositToBankingApi(depositId, request.Amount, "Deposit");

                if (response == null)
                {
                    throw new CustomException(CustomStatusCode.TransactionFailed, "Failed to process the transaction with the banking API.");
                }

                _logger.Info($"Deposit successfully processed. Redirecting User ID: {userId} to Payment URL: {response.PaymentUrl}");
                return response;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task SuccessDeposit([FromBody] Response response)
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
        }
    }
}
