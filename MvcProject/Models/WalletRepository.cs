using Dapper;
using Microsoft.EntityFrameworkCore;
using MvcProject.Models.IRepository;
using System.Data;
namespace MvcProject.Models
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
            var query = "INSERT INTO Wallet (UserId, CurrentBalance, Currency) VALUES (@UserId, @CurrentBalance, @Currency)";
            await _dbConnection.ExecuteAsync(query, new
            {
                UserId = userId,
                CurrentBalance = 0.00M, 
                Currency = 1 
            });
        }

        public async Task<decimal> GetWalletBalanceByUserIdAsync(string userId)
        {
            var query = "Select CurrenctBalance from Wallet";
            return await _dbConnection.QueryFirstOrDefaultAsync<decimal>(query, new { UserId = userId });
        }
        public async Task<int> GetWalletCurrencyByUserIdAsync(string userId)
        {
            var query = "Select CurrenctBalance from Wallet";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query,new { UserId=userId});
        }
    }
}
