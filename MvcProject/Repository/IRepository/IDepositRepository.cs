using MvcProject.Models;
using MvcProject.Models.Enum;

namespace MvcProject.Repository.IRepository
{
    public interface IDepositRepository
    {
        Task RegisterTransaction(Response response);
        Task<int> RegisterDeposit
            (string userId, Status status, TransactionType transactionType, decimal amount);
    }
}