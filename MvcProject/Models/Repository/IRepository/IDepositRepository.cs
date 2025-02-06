using MvcProject.Models.Model;
using MvcProject.Models.Model.DTO;
using MvcProject.Models.Repository.IRepository.Enum;

namespace MvcProject.Models.Repository.IRepository
{
    public interface IDepositRepository
    {
        Task<string> GetUserIdByResponce(Model.Response response);
        Task RegisterTransaction(string userId, Response response);
        Task<int> RegisterDeposit
            (string userId, Status status, TransactionType transactionType, decimal amount);
    }
}