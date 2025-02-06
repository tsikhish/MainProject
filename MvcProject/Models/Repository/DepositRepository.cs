using Dapper;
using log4net;
using MvcProject.Models.Exceptions;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Data;

namespace MvcProject.Models.Repository
{
    public class DepositRepository : IDepositRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DepositRepository));
        private readonly IDbConnection _connection;
        private readonly ICustomExceptions _customException;
        public DepositRepository(ICustomExceptions customExceptions,IDbConnection connection)
        {
            _customException = customExceptions;
            _connection = connection;
        }

        public async Task<int> RegisterDeposit(string userId, Status status, TransactionType transactionType, decimal amount)
        {
            _logger.Info($"Registering deposit for user: {userId}, Amount: {amount}, Status: {status}, TransactionType: {transactionType}");

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@TransactionType", transactionType);
                parameters.Add("@Status", status);
                parameters.Add("@Amount", amount);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@DepositWithdrawId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _connection.ExecuteAsync("AddDeposit", parameters, commandType: CommandType.StoredProcedure);

                var outputParam2Value = parameters.Get<int>("@ReturnCode");
                var depositId = parameters.Get<int>("@DepositWithdrawId");
                await _customException.DepositException(depositId, userId, outputParam2Value);
                _logger.Info($"Successfully registered deposit for user {userId}, DepositWithdrawId: {depositId}");
                return depositId;
            }
            catch (DepositException ex)
            {
                _logger.Error($"Error occurred for User {userId}: {ex.Message}, Error Code: {ex.ErrorCode}");
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred while registering deposit for user {userId}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
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
        public async Task RegisterTransaction(string userId, Response response)
        {
            _logger.Info($"Registering transaction for DepositWithdrawId: {response.DepositWithdrawRequestId}, Amount: {response.Amount}, Status: {response.Status}");

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@TransactionType", TransactionType.Deposit);
                parameters.Add("@Status", response.Status);
                parameters.Add("@Amount", response.Amount);
                parameters.Add("@DepositWithdrawId", response.DepositWithdrawRequestId);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var result = await _connection.ExecuteAsync("AddDepositTransaction", parameters, commandType: CommandType.StoredProcedure);

                var outputParam2Value = parameters.Get<int>("@ReturnCode");

                if (outputParam2Value == 400)
                {
                    _logger.Warn($"Amount mismatch for DepositWithdrawId: {response.DepositWithdrawRequestId}.");
                    throw new Exception("Amount is different");
                }
                else if (outputParam2Value == 401)
                {
                    _logger.Warn($"Status was already changed for DepositWithdrawId: {response.DepositWithdrawRequestId}.");
                    throw new Exception("Status was already changed");
                }
                else if (outputParam2Value == 500)
                {
                    _logger.Error($"Internal error occurred while processing transaction for DepositWithdrawId: {response.DepositWithdrawRequestId}.");
                    throw new Exception("Internal Error");
                }

                _logger.Info($"Successfully registered transaction for DepositWithdrawId: {response.DepositWithdrawRequestId}.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred while registering transaction for DepositWithdrawId: {response.DepositWithdrawRequestId}. Exception: {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}
