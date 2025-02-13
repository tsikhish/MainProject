using Dapper;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MvcProject.Exceptions;
using MvcProject.Hash;
using MvcProject.Models;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using System.Data;

namespace MvcProject.Repository
{
    public class WithdrawRepository : IWithdrawRepository
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDbConnection _connection;
        private readonly string _secretKey;
        private readonly string _merchantId;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WithdrawRepository));
        public WithdrawRepository(IOptions<AppSettings> appSettings, ITransactionRepository transactionRepository,
             IDbConnection connection)
        {
            _merchantId = appSettings.Value.MerchantID;
            _secretKey = appSettings.Value.SecretKey;
            _transactionRepository = transactionRepository;
            _connection = connection;
        }

        public async Task RegisterWithdraw(string userId, Status status, TransactionType transactionType, decimal amount)
        {
            _logger.InfoFormat("Registering withdraw for user {0}, Amount: {1}, Status: {2}, TransactionType: {3}", userId, amount, status, transactionType);
            var outputParam2Value = await AddParamsToDeposit(userId, status, transactionType, amount);
            if (outputParam2Value != 0)
            {
                CustomStatusCode cusstatus = Enum.IsDefined(typeof(CustomStatusCode), outputParam2Value)
                                                          ? (CustomStatusCode)outputParam2Value
                                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(cusstatus, $"Something's Wrong. StatusCode: {outputParam2Value} ({cusstatus})");
            }      
            _logger.InfoFormat("Successfully registered withdraw for UserId {0}.", userId);
        }
        public async Task AddWithdrawTransactionAsync(Models.Response response)
        {
            _logger.InfoFormat("Registering withdraw transaction for Id {0}, Amount: {1}, Status: {2}, DepositWithdrawId: {3}",
                response.DepositWithdrawRequestId, response.Amount, response.Status, response.DepositWithdrawRequestId);
            var returnCode = await AddParamsToTransaction(response);
            if (returnCode != 0)
            {
                CustomStatusCode cusstatus = Enum.IsDefined(typeof(CustomStatusCode), returnCode)
                                                                          ? (CustomStatusCode)returnCode
                                                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(cusstatus, $"Something's Wrong. StatusCode: {returnCode} ({cusstatus})");
            }
            _logger.InfoFormat("Successfully registered withdraw transaction for Id {0}.", response.DepositWithdrawRequestId);
        }

        public async Task<Withdraw> GetWithdrawTransaction(int id)
        {
            _logger.InfoFormat("Fetching withdraw transaction details for transaction ID: {0}.", id);
            var usersFullName = await _transactionRepository.GetUsersFullNameAsync(id);
            var hash = Hash256.ComputeSHA256Hash((int)(usersFullName.Amount * 100), _merchantId, id, usersFullName.UserName, _secretKey);
            var transaction = new Withdraw
            {
                TransactionID = id,
                MerchantID = _merchantId,
                Amount = (int)(usersFullName.Amount * 100),
                Hash = hash,
                UsersFullName = usersFullName.UserName
            };
            _logger.InfoFormat("Successfully fetched withdraw transaction details for transaction ID: {0}.", id);
            return transaction;
        }

        public async Task<string> GetUserIdByResponce(Models.Response response)
        {
            _logger.InfoFormat("Fetching user ID for DepositWithdrawRequestId: {0}.", response.DepositWithdrawRequestId);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("id", response.DepositWithdrawRequestId);
            string userId = await _connection.QuerySingleOrDefaultAsync<string>("GetUserIdByResponce", parameters, commandType: CommandType.StoredProcedure);
            _logger.InfoFormat("Successfully fetched user ID for DepositWithdrawRequestId: {0}.", response.DepositWithdrawRequestId);
            return userId;
        }
        private async Task<int> AddParamsToTransaction(Models.Response response)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", response.DepositWithdrawRequestId, DbType.Int32);
            parameters.Add("@TransactionType", TransactionType.Withdraw, DbType.Int32);
            parameters.Add("@Amount", response.Amount, DbType.Decimal);
            parameters.Add("@Status", response.Status, DbType.Int32);
            parameters.Add("@DepositWithdrawId", response.DepositWithdrawRequestId, DbType.Int32);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _connection.ExecuteAsync("AddWithdrawTransaction", parameters, commandType: CommandType.StoredProcedure);
            int returnCode = parameters.Get<int>("@ReturnCode");
            return returnCode;
        }
        private async Task<int> AddParamsToDeposit(string userId, Status status, TransactionType transactionType, decimal amount)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@TransactionType", transactionType);
            parameters.Add("@Status", status);
            parameters.Add("@Amount", amount);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            var result = _connection.Execute("AddWithdraw", parameters, commandType: CommandType.StoredProcedure);
            var outputParam2Value = parameters.Get<int>("@ReturnCode");
            return outputParam2Value;
        }

    }
}
