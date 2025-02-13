using MvcProject.Models;
using MvcProject.Models.Enum;

namespace MvcProject.Repository.IRepository
{
    public interface IWithdrawRepository
    {
        Task AddWithdrawTransactionAsync
            (Response response);
        Task<string> GetUserIdByResponce(Response response);
        public Task<Withdraw> GetWithdrawTransaction(int id);
        Task RegisterWithdraw
           (string userId, Status status, TransactionType transactionType, decimal amount);
    }
}
