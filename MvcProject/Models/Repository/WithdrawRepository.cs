using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
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
        private readonly IHash256 _hash;
        private readonly ILogger<WithdrawRepository> _logger;

        public WithdrawRepository(IHash256 hash, IOptions<AppSettings> appSettings, ITransactionRepository transactionRepository,
             IDbConnection connection, ILogger<WithdrawRepository> logger)
        {
            _hash = hash;
            _merchantId = appSettings.Value.MerchantID;
            _secretKey = appSettings.Value.SecretKey;
            _transactionRepository = transactionRepository;
            _connection = connection;
            _logger = logger;
        }

        public async Task RegisterWithdraw(string userId, Status status, TransactionType transactionType, decimal amount)
        {
            _logger.LogInformation("Registering withdraw for user {UserId}, Amount: {Amount}, Status: {Status}, TransactionType: {TransactionType}", userId, amount, status, transactionType);
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@TransactionType", transactionType);
                parameters.Add("@Status", status);
                parameters.Add("@Amount", amount);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                var result = _connection.Execute("AddWithdraw", parameters, commandType: CommandType.StoredProcedure);
                var outputParam2Value = parameters.Get<int>("@ReturnCode");
                if (outputParam2Value == 400)
                {
                    _logger.LogWarning("User {UserId} has already sent a pending withdraw request.", userId);
                    throw new Exception("This user has already sent a pending request. Please wait for results.");
                }
                else if (outputParam2Value == 401)
                {
                    _logger.LogWarning("User {UserId} does not have enough balance for withdraw.", userId);
                    throw new Exception("This user doesn't have enough balance in the wallet");
                }
                else if (outputParam2Value == 500)
                {
                    _logger.LogError("Transaction failed for user {UserId}.", userId);
                    throw new Exception("Transaction Failed.");
                }

                _logger.LogInformation("Successfully registered withdraw for user {UserId}.", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering withdraw for user {UserId}.", userId);
                throw new Exception(ex.Message);
            }
        }

        public async Task AddWithdrawTransactionAsync(Response response, string userId)
        {
            _logger.LogInformation("Registering withdraw transaction for user {UserId}, Amount: {Amount}, Status: {Status}, DepositWithdrawId: {DepositWithdrawId}",
                userId, response.Amount, response.Status, response.DepositWithdrawRequestId);
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId, DbType.String);
            parameters.Add("@TransactionType", TransactionType.Withdraw, DbType.Int32);
            parameters.Add("@Amount", response.Amount, DbType.Decimal);
            parameters.Add("@Status", response.Status, DbType.Int32);
            parameters.Add("@DepositWithdrawId", response.DepositWithdrawRequestId, DbType.Int32);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            try
            {
                await _connection.ExecuteAsync("AddWithdrawTransaction", parameters, commandType: CommandType.StoredProcedure);
                int returnCode = parameters.Get<int>("@ReturnCode");

                if (returnCode == 400)
                {
                    _logger.LogWarning("Amount mismatch for user {UserId} on withdraw transaction.", userId);
                    throw new InvalidOperationException("The requested amount does not match the original request.");
                }
                if (returnCode == 401)
                {
                    _logger.LogWarning("Status of withdraw request already changed for user {UserId}.", userId);
                    throw new InvalidOperationException("The status of this withdrawal request has already been changed.");
                }
                if (returnCode == 500)
                {
                    _logger.LogError("Internal error while processing withdraw transaction for user {UserId}.", userId);
                    throw new Exception("An internal error occurred while processing the transaction.");
                }
                _logger.LogInformation("Successfully registered withdraw transaction for user {UserId}.", userId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while registering withdraw transaction for user {UserId}.", userId);
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering withdraw transaction for user {UserId}.", userId);
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }
        public async Task<Withdraw> GetWithdrawTransaction(int id)
        {
            _logger.LogInformation("Fetching withdraw transaction details for transaction ID: {TransactionId}.", id);
            var usersFullName = await _transactionRepository.GetUsersFullNameAsync(id);
            var hash = _hash.ComputeSHA256Hash((int)(usersFullName.Amount * 100), _merchantId, id, usersFullName.UserName, _secretKey);
            var transaction = new Withdraw
            {
                TransactionID = id,
                MerchantID = _merchantId,
                Amount = (int)(usersFullName.Amount * 100),
                Hash = hash,
                UsersFullName = usersFullName.UserName
            };
            _logger.LogInformation("Successfully fetched withdraw transaction details for transaction ID: {TransactionId}.", id);
            return transaction;
        }

        public async Task<string> GetUserIdByResponce(Response response)
        {
            _logger.LogInformation("Fetching user ID for DepositWithdrawRequestId: {DepositWithdrawRequestId}.", response.DepositWithdrawRequestId);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("id", response.DepositWithdrawRequestId);
            try
            {
                string userId = await _connection.QuerySingleOrDefaultAsync<string>("GetUserIdByResponce", parameters, commandType: CommandType.StoredProcedure);
                _logger.LogInformation("Successfully fetched user ID for DepositWithdrawRequestId: {DepositWithdrawRequestId}.", response.DepositWithdrawRequestId);
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user ID for DepositWithdrawRequestId: {DepositWithdrawRequestId}.", response.DepositWithdrawRequestId);
                throw new Exception($"Error fetching user ID: {ex.Message}", ex);
            }
        }
    }
}
