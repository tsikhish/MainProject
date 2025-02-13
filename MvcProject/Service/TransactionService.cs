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
    public class TransactionService : ITransactionService
    {
        private readonly ILog _logger;
        private readonly IDepositRepository _depositRepository;
        private readonly IBankingRequestService _bankingRequestService;
        private readonly ITransactionRepository _transactionRepository;
        public TransactionService(ILoggerFactoryService loggerFactory, ITransactionRepository transactionRepository,
            IDepositRepository depositRepository, IBankingRequestService bankingRequestService)
        {
            _bankingRequestService = bankingRequestService;
            _logger = loggerFactory.GetLogger<TransactionService>();
            _transactionRepository = transactionRepository;
            _depositRepository = depositRepository;
        }
        public async Task<Models.Response> SendingDepositToBankingApi(DepositWithdrawRequest depositWithdrawRequest)
        {
            depositWithdrawRequest.TransactionType = TransactionType.Deposit;
            _logger.InfoFormat("Initiating deposit transaction for UserId: {0}, Amount: {1}", depositWithdrawRequest.UserId, depositWithdrawRequest.Amount);
            var response = await _bankingRequestService.SendDepositToBankingApi(depositWithdrawRequest, "ConfirmDeposit");
            if (response == null)
            {
                _logger.ErrorFormat("Banking API response is null for UserId: {0}.", depositWithdrawRequest.UserId);
                throw new CustomException(CustomStatusCode.TransactionFailed, "Failed to process the transaction with the banking API.");
            }
            return response;
        }
        public async Task<DepositWithdrawRequest> GetDepositWithdraw(int depositWithdrawId, int amount)
        {
            _logger.InfoFormat("Received request for transaction details. DepositWithdrawID: {0}, Amount: {1}", depositWithdrawId, amount);
            if (depositWithdrawId == 0 || amount == 0)
            {
                _logger.ErrorFormat("Invalid arguments. DepositWithdrawID or Amount cannot be zero.");
                throw new Exception("Arguments can't be null. Incorrect URL");
            }
            var transaction = await _transactionRepository.GetDepositWithdrawById(depositWithdrawId);
            if (transaction == null)
            {
                _logger.WarnFormat("Transaction with ID {0} not found.", depositWithdrawId);
                throw new CustomException(CustomStatusCode.NotExists, "Transaction Not Exists");
            }
            _logger.InfoFormat("Transaction retrieved successfully. Transaction ID: {0}", depositWithdrawId);
            return transaction;
        }
        public async Task<IEnumerable<Transactions>> TransactionHistory(string userId)
        {
            var transactions = await _transactionRepository.GetTransactionByUserId(userId);
            if (transactions == null || !transactions.Any())
            {
                _logger.Warn($"No transactions found for user with ID: {userId}");
            }
            else
            {
                _logger.Info($"Successfully retrieved {transactions.Count()} transactions for user with ID: {userId}");
            }
            return transactions;
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> AdminDashboard()
        {
            _logger.Info("Fetching withdrawal transactions for the admin dashboard.");

            var withdraws = await _transactionRepository.GetWithdrawTransactionsForAdmins();
            if (withdraws == null || !withdraws.Any())
            {
                _logger.Warn("No withdrawal transactions found for the admin dashboard.");
            }
            else
            {
                _logger.Info("Successfully retrieved withdrawal transactions for the admin dashboard.");
            }
            return withdraws;
        }
        public async Task UpdateRejectedStatus(int id)
        {
            await _transactionRepository.UpdateRejectedStatus(id);
        }
    }
}
