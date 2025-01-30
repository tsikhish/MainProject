using Dapper;
using Microsoft.Data.SqlClient;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Data;
using System.Transactions;
namespace MvcProject.Models.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ILogger<TransactionRepository> _logger;
        private readonly IDbConnection _connection;

        public TransactionRepository(ILogger<TransactionRepository> logger, IDbConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }
        public async Task UpdateRejectedStatus(int id)
        {
            _logger.LogInformation("Starting UpdateRejectedStatus for Id: {Id}", id);
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", id);
                parameters.Add("@Status", Status.Rejected);
                var result = await _connection.ExecuteAsync(
                    "UpdateRejectedStatus", parameters, commandType: CommandType.StoredProcedure);
                if (result == 0)
                {
                    _logger.LogWarning("No rows were updated for Id: {Id}. It might not exist.", id);
                    throw new Exception($"No rows were updated for Id {id}. It might not exist.");
                }
                _logger.LogInformation("Successfully updated status to Rejected for Id: {Id}", id);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating rejected status for Id: {Id}. Error Code: {ErrorCode}", id, ex.Number);
                if (ex.Number == 50000)
                {
                    throw new Exception($"Error from SQL Server: {ex.Message}");
                }
                else
                {
                    throw new Exception($"A SQL error occurred: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating rejected status for Id: {Id}", id);
                throw new Exception("An error occurred while updating the status.", ex);
            }
        }
        public async Task<TransactionInfo> GetUsersFullNameAsync(int id)
        {
            _logger.LogInformation("Fetching user's full name for Id: {Id}", id);
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                var result = await _connection.QuerySingleOrDefaultAsync<TransactionInfo>(
                    "GetUsersFullName",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                if (result == null)
                {
                    _logger.LogWarning("No user found for Id: {Id}. It might not exist.", id);
                    throw new Exception($"No user found for the given Id {id}. It might not exist.");
                }
                _logger.LogInformation("Successfully retrieved user's full name for Id: {Id}", id);
                return result;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while retrieving user full name for Id: {Id}. Error Code: {ErrorCode}", id, ex.Number);
                if (ex.Number == 50001)
                {
                    throw new Exception($"Error from SQL Server: {ex.Message}");
                }
                else
                {
                    throw new Exception($"A SQL error occurred: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user transaction info for Id: {Id}", id);
                throw new Exception("An error occurred while retrieving the user's transaction info.", ex);
            }
        }
        public async Task<IEnumerable<Transactions>> GetTransactionByUserId(string userId)
        {
            _logger.LogInformation("Fetching transaction history for user: {UserId}", userId);
            var query = "select * from Transactions where UserId=@userId";
            return await _connection.QueryAsync<Transactions>(query, new { UserId = userId });
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins()
        {
            _logger.LogInformation("Fetching pending withdraw transactions for admins.");
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@TransactionType", TransactionType.Withdraw);
                parameters.Add("@Status", Status.Pending);
                var depositWithdraw = await _connection.QueryAsync<DepositWithdrawRequest>(
                    "GetWithdrawTransactions",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                _logger.LogInformation("Successfully fetched {Count} pending withdraw transactions for admins.", depositWithdraw?.Count() ?? 0);
                return depositWithdraw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while fetching withdraw transactions for admins. Error Code: {ErrorCode}", ex.Number);
                throw new Exception($"A SQL error occurred: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching withdraw transactions for admins.");
                throw new Exception("An error occurred while fetching withdraw transactions for admins.", ex);
            }
        }
        public async Task<DepositWithdrawRequest> GetDepositWithdrawById(int id)
        {
            _logger.LogInformation("Fetching deposit/withdraw request with ID: {Id}", id);

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", id);

                var depositWithdraw = await _connection.QuerySingleOrDefaultAsync<DepositWithdrawRequest>(
                    "GetDepositWithdrawById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (depositWithdraw != null)
                {
                    _logger.LogInformation("Successfully fetched deposit/withdraw request with ID: {Id}", id);
                }
                else
                {
                    _logger.LogWarning("No deposit/withdraw request found for ID: {Id}", id);
                }

                return depositWithdraw;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while fetching deposit/withdraw request with ID: {Id}. Error Code: {ErrorCode}", id, ex.Number);
                throw new Exception($"A SQL error occurred: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching deposit/withdraw request with ID: {Id}", id);
                throw new Exception("An error occurred while fetching deposit/withdraw request.", ex);
            }
        }

    }
}

