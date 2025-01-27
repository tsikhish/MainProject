using MvcProject.Models.Model;

namespace MvcProject.Models.Repository.IRepository
{
    public interface IWalletRepository
    {
        Task<decimal> GetWalletBalanceByUserIdAsync(string userId);
        Task<int> GetWalletCurrencyByUserIdAsync(string userId);
        Task CreateWalletByUserIdAsync(string userId);
        Task UpdateWalletAmount(DepositWithdrawRequest deposit);
        Task UpdateWalletAmount(string userID, Response response);
    }
}
