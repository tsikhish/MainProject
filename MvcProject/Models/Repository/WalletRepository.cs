using Dapper;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using Newtonsoft.Json;
using System.Data;
using System.Text;
namespace MvcProject.Models.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly IDbConnection _dbConnection;
        public WalletRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task CreateWalletByUserIdAsync(string userId)
        {
            var existingUserId = "Select UserId from Wallet where UserId=@userId";
            var exist = await _dbConnection.QueryFirstOrDefaultAsync<string>(existingUserId, new { UserId = userId });
            if (exist != null) { throw new Exception("Already exists"); }
            var query = "INSERT INTO Wallet (UserId, CurrentBalance, Currency) VALUES (@UserId, @CurrentBalance, @Currency)";
            await _dbConnection.ExecuteAsync(query, new
            {
                UserId = userId,
                CurrentBalance = 0.00M,
                Currency = 1
            });
        }

        public async Task<int> GetWalletCurrencyByUserIdAsync(string userId)
        {
            var query = "Select Currency from Wallet";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId });
        }

        public async Task<decimal> GetWalletBalanceByUserIdAsync(string userId)
        {
            var query = "Select CurrentBalance from Wallet where UserId=@userId";
            return await _dbConnection.QueryFirstOrDefaultAsync<decimal>(query, new { UserId = userId });
        }
        public async Task UpdateWalletAmount(DepositWithdrawRequest deposit)
        {
            var oldBalance = await GetWalletBalanceByUserIdAsync(deposit.UserId);
            var newBalance = oldBalance + deposit.Amount;
            var query = "Update Wallet set CurrentBalance = @currentbalance where UserId=@UserId";
            await _dbConnection.ExecuteAsync(query, new
            {
                CurrentBalance = newBalance,
                UserId = deposit.UserId,
            });
        }
        public async Task UpdateWalletAmount(string userID, Response response)
        {
            var oldBalance = await GetWalletBalanceByUserIdAsync(userID);
            var newBalance = oldBalance - response.Amount;
            var query = "Update Wallet set CurrentBalance = @currentbalance where UserId=@UserId";
            await _dbConnection.ExecuteAsync(query, new
            {
                CurrentBalance = newBalance,
                UserId = userID
            });
        }

    }

}
