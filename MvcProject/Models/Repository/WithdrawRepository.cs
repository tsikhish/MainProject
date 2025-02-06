using Dapper;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MvcProject.Models.Exceptions;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Data;

namespace MvcProject.Models.Repository
{
    public class WithdrawRepository : IWithdrawRepository
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDbConnection _connection;
        private readonly string _secretKey;
        private readonly string _merchantId;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WithdrawRepository));
        private readonly ICustomExceptions _customExceptions;
        public WithdrawRepository(ICustomExceptions customExceptions, IOptions<AppSettings> appSettings, ITransactionRepository transactionRepository,
             IDbConnection connection)
        {
            _customExceptions = customExceptions;
            _merchantId = appSettings.Value.MerchantID;
            _secretKey = appSettings.Value.SecretKey;
            _transactionRepository = transactionRepository;
            _connection = connection;
        }

        public async Task RegisterWithdraw(string userId, Status status, TransactionType transactionType, decimal amount)
        {
            _logger.InfoFormat("Registering withdraw for user {0}, Amount: {1}, Status: {2}, TransactionType: {3}", userId, amount, status, transactionType);
            try
            {
                var outputParam2Value = await AddParamsToDeposit(userId,status,transactionType,amount);
                await _customExceptions.WithdrawExceptions(outputParam2Value, userId);
                _logger.InfoFormat("Successfully registered withdraw for user {0}.", userId);
            }
            catch (WithdrawException ex)
            {
                _logger.Error($"Error occurred for User {userId}: {ex.Message}, Error Code: {ex.ErrorCode}");
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error occurred while registering withdraw for user {0}. Exception: {1}", userId, ex);
                throw new Exception(ex.Message);
            }
        }
        public async Task AddWithdrawTransactionAsync(Model.Response response, string userId)
        {
            _logger.InfoFormat("Registering withdraw transaction for user {0}, Amount: {1}, Status: {2}, DepositWithdrawId: {3}",
                userId, response.Amount, response.Status, response.DepositWithdrawRequestId);
            try
            {
                var returnCode = await AddParamsToTransaction(userId, response);
                await _customExceptions.TransactionWithdrawException(returnCode, userId);
                _logger.InfoFormat("Successfully registered withdraw transaction for user {0}.", userId);
            }
            catch (SqlException ex)
            {
                _logger.ErrorFormat("Database error while registering withdraw transaction for user {0}. Exception: {1}", userId, ex);
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error occurred while registering withdraw transaction for user {0}. Exception: {1}", userId, ex);
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
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

        public async Task<string> GetUserIdByResponce(Model.Response response)
        {
            _logger.InfoFormat("Fetching user ID for DepositWithdrawRequestId: {0}.", response.DepositWithdrawRequestId);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("id", response.DepositWithdrawRequestId);
            try
            {
                string userId = await _connection.QuerySingleOrDefaultAsync<string>("GetUserIdByResponce", parameters, commandType: CommandType.StoredProcedure);
                _logger.InfoFormat("Successfully fetched user ID for DepositWithdrawRequestId: {0}.", response.DepositWithdrawRequestId);
                return userId;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error occurred while fetching user ID for DepositWithdrawRequestId: {0}. Exception: {1}", response.DepositWithdrawRequestId, ex);
                throw new Exception($"Error fetching user ID: {ex.Message}", ex);
            }
        }
        private async Task<int> AddParamsToTransaction(string userId, Model.Response response)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.String);
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
