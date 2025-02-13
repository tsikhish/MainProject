using MvcProject.Models;

namespace MvcProject.Repository.IRepository
{
    public interface IWalletRepository
    {
        Task<decimal> GetWalletBalanceByUserIdAsync(string userId);
        Task<int> GetWalletCurrencyByUserIdAsync(string userId);
        Task CreateWalletByUserIdAsync(string userId);
    }
}
