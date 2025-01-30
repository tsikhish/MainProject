using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using Newtonsoft.Json;
using System.Data;
using System.Security.Policy;
using System.Text;
using System.Transactions;

namespace MvcProject.Models.Repository
{
    public class WithdrawRepository : IWithdrawRepository
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDbConnection _connection;
        private readonly string _secretKey;
        private readonly string _merchantId;
        private readonly IHash256 _hash;

        public WithdrawRepository(IHash256 hash, IOptions<AppSettings> appSettings, ITransactionRepository transactionRepository,
             IDbConnection connection)
        {
            _hash = hash;
            _merchantId = appSettings.Value.MerchantID;
            _secretKey = appSettings.Value.SecretKey;
            _transactionRepository = transactionRepository;
            _connection = connection;
        }
        public async Task   RegisterWithdraw
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
                var result = _connection.Execute(
                    "AddWithdraw", parameters, commandType: CommandType.StoredProcedure);
                var outputParam2Value = parameters.Get<int>("@ReturnCode");
                if (outputParam2Value == 400)
                    throw new Exception("This user has already sent a pending request. Please wait for results.");
                else if (outputParam2Value == 401)
                    throw new Exception("This user doesnt have enough balance on the wallet");
                else if (outputParam2Value == 500)
                    throw new Exception("Transaction Failed.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public async Task AddWithdrawTransactionAsync(Response response,string userId)
        {
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
                    throw new InvalidOperationException("The requested amount does not match the original request.");
                }
                if (returnCode == 401)
                {
                    throw new InvalidOperationException("The status of this withdrawal request has already been changed.");
                }
                if (returnCode == 500)
                {
                    throw new Exception("An internal error occurred while processing the transaction.");
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }
        public async Task<Withdraw> GetWithdrawTransaction(int id)
        {
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
            return transaction;
        }
        public async Task<string> GetUserIdByResponce(Response response)
        {
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("id", response.DepositWithdrawRequestId);

            return await _connection.QuerySingleOrDefaultAsync<string>("GetUserIdByResponce", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
