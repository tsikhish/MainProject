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
        private readonly IDbConnection _connection;
        public TransactionRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task UpdateRejectedStatus(int id)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", id);
                parameters.Add("@Status", Status.Rejected);
                var result = await _connection.ExecuteAsync(
                    "UpdateRejectedStatus", parameters, commandType: CommandType.StoredProcedure);
                if (result == 0)
                {
                    throw new Exception($"No rows were updated for Id {id}. It might not exist.");
                }
            }
            catch (SqlException ex)
            {
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
                throw new Exception("An error occurred while updating the status.", ex);
            }
        }

        public async Task<TransactionInfo> GetUsersFullNameAsync(int id)
        {
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
                    throw new Exception($"No user found for the given Id {id}. It might not exist.");
                }
                return result; 
            }
            catch (SqlException ex)
            {
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
                throw new Exception("An error occurred while retrieving the user's transaction info.", ex);
            }
        }
        public async Task<IEnumerable<Transactions>> GetTransactionByUserId(string userId)
        {
            var query = "select * from Transactions where UserId=@userId";
            return await _connection.QueryAsync<Transactions>(query, new { UserId = userId });
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins()
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@TransactionType", TransactionType.Withdraw);
            parameters.Add("@Status",Status.Pending);
            var depositWithdraw =await _connection.QueryAsync<DepositWithdrawRequest>("GetWithdrawTransactions", parameters, commandType: CommandType.StoredProcedure);
            return depositWithdraw;
        }
        public async Task<DepositWithdrawRequest> GetDepositWithdrawById(int id)
        {
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@id", id);
                var depositWithdraw = _connection.QuerySingleOrDefault<DepositWithdrawRequest>("GetDepositWithdrawById", parameters, commandType: CommandType.StoredProcedure);
                return depositWithdraw;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
