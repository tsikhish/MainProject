using Dapper;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;
using Newtonsoft.Json;
using System.Data;
using System.Text;

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
            var currentAmountQuery = "SELECT CurrentBalance FROM Wallet WHERE UserId = @UserId";
            var currentAmount = await _connection.QuerySingleOrDefaultAsync<decimal>(currentAmountQuery, new
            {
                UserId = userId
            });
            if (currentAmount < amount)
            {
                throw new Exception("Amount must be less than or equal to the current wallet balance.");
            }
            var query = "EXEC AddDepositWithdraw @UserId,@Amount, @TransactionType,  @Status";
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

        public async Task RegisterTransactionInTransactionsAsync(Deposit deposit)
        {
            var query = "Exec RegisterTransaction @UserId,@Amount,@Status";
            var transaction = new Transactions
            {
                UserId = deposit.MerchantID,
                Status = Status.Success,
                Amount = deposit.Amount,
            };
        }
         public async Task UpdateTransactionAsync(DepositWithdrawRequest transaction)
        {
            var query = "Update DepositWithdrawRequest set status=@Status where Id = @Id";
            await _connection.ExecuteAsync(query, new
            {
                Status = Status.Rejected,
                Id=transaction.Id
            });
        }
        public async Task UpdateDepositTable(Deposit deposit)
        {
            var depositQuery = "Update DepositWithdrawRequest set status = @status where Id = @Id";
            await _connection.ExecuteAsync(depositQuery, new
            {
                Status = Status.Success,
                Id = deposit.DepositWithdrawId
            });
        }
        public async Task GetTransactionByDepositWithdrawId(Deposit deposit)
        {
            var query = "Select * from DepositWithdraw where Id=@Id";
            await _connection.ExecuteAsync(query, new
            {
                Id = deposit.DepositWithdrawId
            });
        }
        public async Task UpdateWalletAmount(Deposit deposit)
        {
            var oldBalanceQuery = "Select CurrentBalance from Wallet where UserId=@UserId";
            var oldBalance = await _connection.QuerySingleOrDefaultAsync<decimal>(oldBalanceQuery, new
            {
                UserId = deposit.MerchantID
            });
            var newBalance = oldBalance - deposit.Amount;
            var query = "Update Wallet set CurrentBalance = @currentbalance where UserId=@UserId";
            await _connection.ExecuteAsync(query, new
            {
                CurrentBalance = newBalance,
                UserId = deposit.MerchantID
            });
        }
        public async Task UpdateSuccessTransactionAsync(Response response)
        {
            var query = "Update Transactions set status=@Status where Id=@Id";
            await _connection.ExecuteAsync(query, new
            {
                Id = response.DepositWithdrawRequestId,
                Status = Status.Success,
            });
        }
        public async Task<Deposit>  SendToBankingApi(Deposit deposit)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                 _connection.Open();
            }

            using (var transactionScope = _connection.BeginTransaction())
            {
                try
                {
                    using var client = new HttpClient();
                    var content = new StringContent(JsonConvert.SerializeObject(deposit), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://localhost:7133/Transactions/Deposit", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var body = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Banking API returned error:{body}, {response.StatusCode} - {response.ReasonPhrase}");
                    }
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Deposit>(responseBody);
                    if (result == null)
                    {
                        throw new Exception("Failed to deserialize Banking API response.");
                    }
                    transactionScope.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    transactionScope.Rollback();
                    throw new Exception("Transaction failed: " + ex.Message);
                }
                finally
                {
                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                }
            }
        }
        
        public async Task<IEnumerable<DepositWithdrawRequest>> GetTransactionByUserId(string userId)
        {
            var query = "select * from DepositWithdrawRequest where UserId=@userId";
            return await _connection.QueryAsync<DepositWithdrawRequest>(query, new { UserId = userId });
        }
        public async Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins()
        {
            var query = "select d.* from DepositWithdrawRequest d" +
                 " join TransactionHistory t on t.TransactionId = d.Id" +
                 " where d.TransactionType = @transactiontype" +
                 " and d.Status = @status" +
                 " and t.SentToBankingApi = 0";
            return await _connection.QueryAsync<DepositWithdrawRequest>(query,
                new { TransactionType = TransactionType.Withdraw, Status = Status.Pending });

        }
        private async Task BoolForBankingApi(int transactionId)
        {
            var query = "INSERT INTO Transactions (Id, TransactionI, SentToBankingApi" +
            "VALUES (@id, @transactionId, @sentToBankingApi";
            var sent = new TransactionsHistory()
            {
                TransactionId = transactionId,
                SentToBankingApi = true
            };
            await _connection.ExecuteAsync(query, sent);
        }

    }
}
