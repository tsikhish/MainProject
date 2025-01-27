using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Transactions;

namespace MvcProject.Models.Repository.IRepository
{
    public interface ITransactionRepository
    {
        Task<int> RegisterDepositWithdraw
               (string userId, Status status, TransactionType transactionType, decimal amount);
        Task RegisterTransactionInTransactionsAsync(string userId, Response response);
        Task UpdateStatus(int id, Status status);
        Task<DepositWithdrawRequest> GetDepositWithdrawById(int id);
        Task<string> GetFullUsername(string userId);
        Task<IEnumerable<Transactions>> GetTransactionByUserId(string userId);
        Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins();
    }
}
