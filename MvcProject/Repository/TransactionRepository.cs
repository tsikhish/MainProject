using Dapper;
using MvcProject.Exceptions;
using MvcProject.Models;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using System.Data;
namespace MvcProject.Repository
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
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@id", id);
            parameters.Add("@Status", Status.Rejected);
            parameters.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await _connection.ExecuteAsync(
                "UpdateRejectedStatus", parameters, commandType: CommandType.StoredProcedure);
            var statusCode = parameters.Get<int>("@StatusCode");
            if (statusCode != 200)
            {
                CustomStatusCode status = Enum.IsDefined(typeof(CustomStatusCode), statusCode)
                                          ? (CustomStatusCode)statusCode
                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(status, $"Something's Wrong. StatusCode: {statusCode} ({status})");
            }
            _logger.LogInformation("Successfully updated status to Rejected for Id: {Id}", id);
        }
        public async Task<TransactionInfo> GetUsersFullNameAsync(int id)
        {
            _logger.LogInformation("Fetching user's full name for Id: {Id}", id);
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@Amount", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@UserName", dbType: DbType.String, size: 450, direction: ParameterDirection.Output);
            await _connection.ExecuteAsync("GetUsersFullName", parameters, commandType: CommandType.StoredProcedure);
            var statusCode = parameters.Get<int>("@ReturnCode");
            var userName = parameters.Get<string>("@UserName");
            var amount = parameters.Get<decimal>("@Amount");
            if (statusCode == 200)
                return new TransactionInfo { Amount = amount, UserName = userName };
            else
            {
                CustomStatusCode status = Enum.IsDefined(typeof(CustomStatusCode), statusCode)
                           ? (CustomStatusCode)statusCode
                           : CustomStatusCode.InternalServerError;

                throw new CustomException(status, $"Something's Wrong. StatusCode: {statusCode} ({status})");
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
        public async Task<DepositWithdrawRequest> GetDepositWithdrawById(int id)
        {
            _logger.LogInformation("Fetching deposit/withdraw request with ID: {Id}", id);

            var parameters = new DynamicParameters();
            parameters.Add("@id", id);

            var depositWithdraw = await _connection.QuerySingleOrDefaultAsync<DepositWithdrawRequest>(
                "GetDepositWithdrawById",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return depositWithdraw;
        }

    }
}

