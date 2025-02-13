using Dapper;
using log4net;
using MvcProject.Controllers;
using MvcProject.Exceptions;
using MvcProject.Models;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using MvcProject.Service;
using System.Data;
namespace MvcProject.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ILog _logger;
        private readonly IDbConnection _connection;

        public TransactionRepository(ILoggerFactoryService loggerFactory,IDbConnection connection)
        {
            _logger = loggerFactory.GetLogger<TransactionsController>();
            _connection = connection;
        }
        public async Task UpdateRejectedStatus(int id)
        {
            _logger.Info("Starting UpdateRejectedStatus");
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
            _logger.Info("Successfully rejected status");
        }
        public async Task<TransactionInfo> GetUsersFullNameAsync(int id)
        {
            _logger.Info("Fetching user's full name");
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
            _logger.Info("Fetching transaction history");
            var query = "select * from Transactions where UserId=@userId";
            return await _connection.QueryAsync<Transactions>(query, new { UserId = userId });
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins()
        {
            _logger.Info("Fetching pending withdraw transactions for admins.");
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@TransactionType", TransactionType.Withdraw);
            parameters.Add("@Status", Status.Pending);
            var depositWithdraw = await _connection.QueryAsync<DepositWithdrawRequest>(
                "GetWithdrawTransactions",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            _logger.Info("Successfully fetched pending withdraw transactions for admins.");
            return depositWithdraw;
        }
        public async Task<DepositWithdrawRequest> GetDepositWithdrawById(int id)
        {
            _logger.Info("Fetching deposit/withdraw request");

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

