namespace MvcProject.Models.IRepository
{
    public interface IWalletRepository
    {
        Task<decimal> GetWalletBalanceByUserIdAsync(string userId);
        Task<int> GetWalletCurrencyByUserIdAsync(string userId);
        Task CreateWalletByUserIdAsync(string userId);
        //Task WalletDeposit(string userId,decimal amount);
        //Task WalletWithdraw(string userId,decimal amount);
    }
}
