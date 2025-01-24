using Dapper;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
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
        public async Task<int> RegisterDepositWithdraw
               (string userId, Status status, TransactionType transactionType, decimal amount)
        {
            try
            {
                if (transactionType == TransactionType.Withdraw)
                {
                    var currentAmountQuery = "SELECT CurrentBalance FROM Wallet WHERE UserId = @UserId";
                    var balance = await _connection.ExecuteScalarAsync<decimal>(currentAmountQuery, new { UserId = userId });
                    if (balance < amount)
                        throw new Exception($"{amount} should be less than the current balance.");
                }
                var statusUserIdQuery = "SELECT Status FROM DepositWithdrawRequest WHERE UserId = @UserId";
                var currentStatus = await _connection.QueryFirstOrDefaultAsync<int>(statusUserIdQuery, new { UserId = userId });
                if (currentStatus == 1)
                    throw new Exception("This userId has already sent a request. Please wait for results.");
                var query = "EXEC AddDepositWithdraw @UserId, @TransactionType, @Amount, @Status";
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
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task UpdateStatus(int id, Status status)
        {
            try
            {
                var depositQuery = "UPDATE DepositWithdrawRequest SET Status = @Status WHERE Id = @Id";
                await _connection.ExecuteAsync(depositQuery, new
                {
                    Id = id,
                    Status = status,
                });
            }
            catch (Exception ex)
            {
                throw;
            }
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
        
        public async Task<string> GetFullUsername(string userId)
        {
            try
            {
                var query = "SELECT UserName FROM dbo.AspNetUsers WHERE Id = @id";
                return await _connection.QuerySingleOrDefaultAsync<string>(query, new { Id = userId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<IEnumerable<Transactions>> GetTransactionByUserId(string userId)
        {
            var query = "select * from Transactions where UserId=@userId";
            return await _connection.QueryAsync<Transactions>(query, new { UserId = userId });
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

        public async Task RegisterTransactionInTransactionsAsync(string userId, Response response)
        {
            var query = "Exec RegisterTransaction @UserId,@Amount,@Status";
            await _connection.ExecuteAsync(query, new
            {
                UserId = userId,
                Amount = response.Amount,
                Status = response.Status
            });
        }
    }
}
