using Dapper;
using log4net;
using Microsoft.AspNetCore.Http;
using MvcProject.Exceptions;
using MvcProject.Models;
using MvcProject.Models.Enum;
using MvcProject.Repository.IRepository;
using System.Data;

namespace MvcProject.Repository
{
    public class DepositRepository : IDepositRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DepositRepository));
        private readonly IDbConnection _connection;
        public DepositRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<int> RegisterDeposit(string userId, Status status, TransactionType transactionType, decimal amount)
        {
            _logger.Info($"Registering deposit for user: {userId}, Amount: {amount}, Status: {status}, TransactionType: {transactionType}");
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
            if (outputParam2Value != 0)
            {
                CustomStatusCode customstatus = Enum.IsDefined(typeof(CustomStatusCode), outputParam2Value)
                                          ? (CustomStatusCode)outputParam2Value
                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(customstatus, $"Something's Wrong. StatusCode: {outputParam2Value} ({customstatus})");
            }
            _logger.Info($"Successfully registered deposit for user {userId}, DepositWithdrawId: {depositId}");
            return depositId;
        }
        public async Task RegisterTransaction(Response response)
        {
            _logger.Info($"Registering transaction for DepositWithdrawId: {response.DepositWithdrawRequestId}, Amount: {response.Amount}, Status: {response.Status}");
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionType", TransactionType.Deposit);
            parameters.Add("@Status", response.Status);
            parameters.Add("@Amount", response.Amount);
            parameters.Add("@DepositWithdrawId", response.DepositWithdrawRequestId);
            parameters.Add("@ReturnCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connection.ExecuteAsync("AddDepositTransaction", parameters, commandType: CommandType.StoredProcedure);

            var outputParam2Value = parameters.Get<int>("@ReturnCode");
            var returnCode = parameters.Get<int>("@ReturnCode");

            if (returnCode != 0)
            {
                CustomStatusCode status = Enum.IsDefined(typeof(CustomStatusCode), returnCode)
                                          ? (CustomStatusCode)returnCode
                                          : CustomStatusCode.InternalServerError;

                throw new CustomException(status, $"Something's Wrong. StatusCode: {returnCode} ({status})");
            }
        }
    }
}
