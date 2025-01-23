using Dapper;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Text;
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
        public async Task<int> RegisterTransactionInDepositTableAsync
                (string userId, Status status, TransactionType transactionType, decimal amount)
        {
            if(transactionType== TransactionType.Withdraw)
            {
                var currentAmount = "select CurrentBalance from Wallet where UserId=@UserId";
                var balance = await _connection.ExecuteScalarAsync<decimal>(currentAmount, new { UserId = userId });
                if (balance < amount) throw new Exception($"{amount} should be less than currentbalance");
            }
            var query = "EXEC AddDepositWithdraw @UserId,@TransactionType, @Amount,  @Status";
            var depositWithdrawRequest = new
            {
                UserId = userId,
                TransactionType = transactionType,
                Amount = amount,
                Status = status
            };
            var id = await _connection.ExecuteScalarAsync<int>(query, depositWithdrawRequest);
            return id;
        }

        public async Task RegisterSuccessTransactionInTransactionsAsync(Deposit deposit)
        {
            var query = "Exec RegisterTransaction @UserId,@Amount,@Status";
            await _connection.ExecuteAsync(query, new
            {
                UserId = deposit.MerchantID,
                Amount = deposit.Amount,
                Status = Status.Success,
            });
        }
         
        public async Task UpdateSuccessDepositTable(Deposit deposit)
        {
            var depositQuery = "Update DepositWithdrawRequest set status = @status where Id = @Id";
            await _connection.ExecuteAsync(depositQuery, new
            {
                Status = Status.Success,
                Id = deposit.TransactionID
            });
        }
        public async Task UpdateWalletAmount(Deposit deposit)
        {
            var oldBalanceQuery = "Select CurrentBalance from Wallet where UserId=@UserId";
            var oldBalance = await _connection.QuerySingleOrDefaultAsync<decimal>(oldBalanceQuery, new
            {
                UserId = deposit.MerchantID
            });
            var newBalance = oldBalance + deposit.Amount;
            var query = "Update Wallet set CurrentBalance = @currentbalance where UserId=@UserId";
            await _connection.ExecuteAsync(query, new
            {
                CurrentBalance = newBalance,
                UserId = deposit.MerchantID
            });
        }
        public async Task UpdateSuccessWithdrawTable(Response response)
        {
            var status = "Update DepositWithdrawRequest set Status =@Status where Id=@id";
            await _connection.ExecuteAsync(status, new {Status = Status.Success,Id=response.DepositWithdrawRequestId});
        }
        public async Task UpdateWalletAmount(string userID,Response response)
        {

            var oldBalanceQuery = "Select CurrentBalance from Wallet where UserId=@UserId";
            var oldBalance = await _connection.QuerySingleOrDefaultAsync<decimal>(oldBalanceQuery, new
            {
                UserId = userID
            });
            var newBalance = oldBalance - response.Amount;
            var query = "Update Wallet set CurrentBalance = @currentbalance where UserId=@UserId";
            await _connection.ExecuteAsync(query, new
            {
                CurrentBalance = newBalance,
                UserId = userID
            });
        }
        public async Task<Response> SendWithdrawToBankingApi(Withdraw withdraw)
        {
            try
            {
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(withdraw), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://localhost:7133/Transactions/ConfirmWithdraw", content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Banking API returned error:{body}, {response.StatusCode} - {response.ReasonPhrase}");
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(responseBody);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize Banking API response.");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Transaction failed: " + ex.Message);
            }

        }
        public async Task<Response> SendToBankingApi(Deposit deposit, string action)
         {
            try
            {
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(deposit), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://localhost:7133/Transactions/{action}", content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Banking API returned error:{body}, {response.StatusCode} - {response.ReasonPhrase}");
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(responseBody);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize Banking API response.");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Transaction failed: " + ex.Message);
            }
        }
        public async Task UpdateWithdrawStatus(int id, Status status)
        {
            var query = "update from DepositWithdrawRequest set Status=@status where id=@id";
            await _connection.ExecuteAsync(query, new {status = status, id = id});
        }
        public async Task<string> GetUserIdByResponse(Response response)
        {
            var query = "Select Id from DepositWithdrawRequest where Id=@id";
            var withdrawId = await _connection.QueryFirstOrDefaultAsync<int>
                (query, new { Id = response.DepositWithdrawRequestId });
            var userId = "select userId from DepositWithdrawRequest where Id=@id";
            return await _connection.QueryFirstOrDefaultAsync<string>(userId, new
            {
                Id = withdrawId });
        }      
        
        public async Task<string> GetFullUsername(string userId)
        {
            var query = "SELECT UserName FROM dbo.AspNetUsers WHERE Id = @id";
            return await _connection.QuerySingleOrDefaultAsync<string>(query, new { Id = userId });
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> GetTransactionByUserId(string userId)
        {
            var query = "select * from DepositWithdrawRequest where UserId=@userId";
            return await _connection.QueryAsync<DepositWithdrawRequest>(query, new { UserId = userId });
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins()
        {
            var query = "select * from DepositWithdrawRequest " +
                 " where TransactionType = @transactiontype" +
                 " and Status = @status";
            return await _connection.QueryAsync<DepositWithdrawRequest>(query,
                new { TransactionType = TransactionType.Withdraw, Status = Status.Pending});

        }
        public async Task<DepositWithdrawRequest> GetDepositWithdrawById(int id)
        {
            var query = "Select * from DepositWithdrawRequest where id=@id";
            return await _connection.QuerySingleOrDefaultAsync<DepositWithdrawRequest>(query, new { Id = id });
        }

        public async Task<DepositWithdrawRequest> FindWithdraw(int id)
        {
            var query = "Select * from DepositWithdrawRequest where id=@id";
            return await _connection.QueryFirstOrDefaultAsync<DepositWithdrawRequest>(query, new { Id = id });
        }

        public async Task UpdateRejectWithdraw(int id)
        {
            var depositQuery = "Update DepositWithdrawRequest set status = @status where Id = @Id";
            await _connection.ExecuteAsync(depositQuery, new
            {
                Id = id,
                Status = Status.Rejected,
            });
        }

        public async Task RegisterRejectedTransactionInTransactionsAsync(string userId,Response response)
        {
            var query = "Exec RegisterTransaction @UserId,@Amount,@Status";
            await _connection.ExecuteAsync(query, new
            {
                UserId = userId,
                Amount = response.Amount,
                Status = Status.Success,
            });
        }
    }
}
