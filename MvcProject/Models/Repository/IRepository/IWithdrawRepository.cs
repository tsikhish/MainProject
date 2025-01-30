using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository.Enum;

namespace MvcProject.Models.Repository.IRepository
{
    public interface IWithdrawRepository
    {
        Task AddWithdrawTransactionAsync
            (Response response, string userId);
        Task<string> GetUserIdByResponce(Response response);
        public Task<Withdraw> GetWithdrawTransaction(int id);
        Task RegisterWithdraw
           (string userId, Status status, TransactionType transactionType, decimal amount);
    }
}
