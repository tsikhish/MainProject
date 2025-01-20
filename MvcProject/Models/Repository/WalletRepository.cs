using Dapper;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models.IRepository;
using MvcProject.Models.IRepository.Enum;
using System.Data;
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
            var query = "Select CurrenctBalance from Wallet";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { UserId = userId });
        }

        //public async Task WalletWithdraw(string userId, decimal amount)
        //{
        //    var currentBalance = await GetWalletBalanceByUserIdAsync(userId);
        //    var newBalance = currentBalance + amount;
        //    var query = $"update wallet set CurrentBalance = @newBalance where UserId=@userId "; 
        //    await _dbConnection.ExecuteAsync(query, new 
        //    {
        //        UserId=userId,
        //        newBalance = newBalance,
        //    });
        //}

        //public async Task WalletDeposit(string userId, decimal amount)
        //{
        //    var currentBalance = await GetWalletBalanceByUserIdAsync(userId);
        //    if (currentBalance > amount)
        //    {
        //        var newBalance = currentBalance - amount;
        //        var query = $"update wallet set CurrentBalance = @newBalance where UserId=@userId ";
        //        await _dbConnection.ExecuteAsync(query, new
        //        {
        //            userId = userId,
        //            newBalance = newBalance,
        //        });
        //    }
        //}
        public async Task<decimal> GetWalletBalanceByUserIdAsync(string userId)
        {
            var query = "Select CurrentBalance from Wallet where UserId=@userId";
            return await _dbConnection.QueryFirstOrDefaultAsync<decimal>(query, new { UserId = userId });
        }
    }
}
