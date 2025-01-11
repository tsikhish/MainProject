namespace MvcProject.Models.IRepository
{
    public interface IWalletRepository
    {
        public Task<decimal> GetWalletBalanceByUserIdAsync(string userId);
        public Task<int> GetWalletCurrencyByUserIdAsync(string userId);
        public Task CreateWalletByUserIdAsync(string userId);
    }
}
