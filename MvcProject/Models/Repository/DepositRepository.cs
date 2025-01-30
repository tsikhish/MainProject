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
        private readonly IDbConnection _connection;
        public DepositRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<int> RegisterDeposit
             (string userId, Status status, TransactionType transactionType, decimal amount)
        {
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
                    throw new Exception("Failed to retrieve DepositWithdrawId.");
                if (outputParam2Value == 400)
                    throw new Exception("This user has already sent a pending request. Please wait for results.");
                else if (outputParam2Value == 401)
                    throw new Exception("Insufficient Balance");
                else if (outputParam2Value == 402)
                    throw new Exception("BlockedAmount update failed");
                else if (outputParam2Value == 500)
                    throw new Exception("Transaction Failed.");
                return depositId; 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task RegisterTransaction(DepositWithdrawRequest deposit,Response response)
        {
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
                    throw new Exception("Amount is different");
                else if (outputParam2Value == 401)
                    throw new Exception("Status was already changed");
                else if (outputParam2Value == 500)
                    throw new Exception("Internal Error");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
       


    }
}
