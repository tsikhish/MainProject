using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository.Enum;
using System.Transactions;

namespace MvcProject.Models.Repository.IRepository
{
    public interface ITransactionRepository
    {
        Task UpdateRejectedStatus(int id);
        Task<DepositWithdrawRequest> GetDepositWithdrawById(int id);
        Task<TransactionInfo> GetUsersFullNameAsync(int id);
        Task<IEnumerable<Transactions>> GetTransactionByUserId(string userId);
        Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins();
    }
}
