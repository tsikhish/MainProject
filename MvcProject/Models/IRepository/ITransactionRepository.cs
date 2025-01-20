using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;

namespace MvcProject.Models.IRepository
{
    public interface ITransactionRepository
    {
        Task<int> RegisterTransactionInDepositTableAsync(string userId, Status status, TransactionType transactionType, decimal amount);
        Task RegisterTransactionInTransactionsAsync(Deposit deposit);
        Task UpdateWalletAmount(Deposit deposit);
        Task<Deposit> SendToBankingApi(Deposit transaction);
        Task UpdateSuccessTransactionAsync(Response response);
        Task GetTransactionByDepositWithdrawId(Deposit deposit);
        Task UpdateDepositTable(Deposit deposit);
        Task UpdateTransactionAsync(DepositWithdrawRequest transaction);
        Task<IEnumerable<DepositWithdrawRequest>> GetTransactionByUserId(string userId);
        Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins();
    }
}
