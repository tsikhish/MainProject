using Dapper;
using Microsoft.Extensions.Options;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Model.DTO;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace MvcProject.Models.Repository
{
    public class DepositRepository : IDepositRepository
    {
        private readonly ILogger<DepositRepository> _logger;
        private readonly IDbConnection _connection;
        public DepositRepository(ILogger<DepositRepository> logger,IDbConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }
        public async Task<int> RegisterDeposit(
    string userId, Status status, TransactionType transactionType, decimal amount)
        {
            _logger.LogInformation("Registering deposit for user: {UserId}, Amount: {Amount}, Status: {Status}, TransactionType: {TransactionType}", userId, amount, status, transactionType);
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@TransactionType", transactionType);
                parameters.Add("@Status", status);
                parameters.Add("@Amount", amount);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@DepositWithdrawId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _connection.ExecuteAsync(
                    "AddDeposit", parameters, commandType: CommandType.StoredProcedure);

                var outputParam2Value = parameters.Get<int>("@ReturnCode");
                var depositId = parameters.Get<int>("@DepositWithdrawId");

                if (depositId == 0)
                {
                    _logger.LogError("Failed to retrieve DepositWithdrawId for user {UserId}.", userId);
                    throw new Exception("Failed to retrieve DepositWithdrawId.");
                }
                if (outputParam2Value == 400)
                {
                    _logger.LogWarning("User {UserId} already has a pending request.", userId);
                    throw new Exception("This user has already sent a pending request. Please wait for results.");
                }
                else if (outputParam2Value == 401)
                {
                    _logger.LogWarning("User {UserId} has insufficient balance.", userId);
                    throw new Exception("Insufficient Balance");
                }
                else if (outputParam2Value == 402)
                {
                    _logger.LogError("Failed to update BlockedAmount for user {UserId}.", userId);
                    throw new Exception("BlockedAmount update failed");
                }
                else if (outputParam2Value == 500)
                {
                    _logger.LogError("Transaction failed for user {UserId}.", userId);
                    throw new Exception("Transaction Failed.");
                }
                _logger.LogInformation("Successfully registered deposit for user {UserId}, DepositWithdrawId: {DepositWithdrawId}", userId, depositId);
                return depositId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering deposit for user {UserId}.", userId);
                throw new Exception(ex.Message);
            }
        }

        public async Task RegisterTransaction(DepositWithdrawRequest deposit, Response response)
        {
            _logger.LogInformation("Registering transaction for DepositWithdrawId: {DepositWithdrawId}, Amount: {Amount}, Status: {Status}", deposit.Id, deposit.Amount, response.Status);

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", deposit.UserId);
                parameters.Add("@TransactionType", TransactionType.Deposit);
                parameters.Add("@Status", response.Status);
                parameters.Add("@Amount", deposit.Amount);
                parameters.Add("@DepositWithdrawId", deposit.Id);
                parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var result = await _connection.ExecuteAsync(
                    "AddDepositTransaction", parameters, commandType: CommandType.StoredProcedure);

                var outputParam2Value = parameters.Get<int>("@ReturnCode");

                if (outputParam2Value == 400)
                {
                    _logger.LogWarning("Amount mismatch for DepositWithdrawId: {DepositWithdrawId}.", deposit.Id);
                    throw new Exception("Amount is different");
                }
                else if (outputParam2Value == 401)
                {
                    _logger.LogWarning("Status was already changed for DepositWithdrawId: {DepositWithdrawId}.", deposit.Id);
                    throw new Exception("Status was already changed");
                }
                else if (outputParam2Value == 500)
                {
                    _logger.LogError("Internal error occurred while processing transaction for DepositWithdrawId: {DepositWithdrawId}.", deposit.Id);
                    throw new Exception("Internal Error");
                }

                _logger.LogInformation("Successfully registered transaction for DepositWithdrawId: {DepositWithdrawId}.", deposit.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering transaction for DepositWithdrawId: {DepositWithdrawId}.", deposit.Id);
                throw new Exception(ex.Message);
            }
        }



    }
}
