using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository.Enum;

namespace MvcProject.Models.Service
{
    public interface IBankingRequestService
    {
        Task<Response> SendDepositToBankingApi(DepositWithdrawRequest amount, string action);
        Task<Response> SendDepositToBankingApi(int depositId,decimal amount, string action);
        Task<Response> SendWithdrawToBankingApi(Withdraw withdraw);
    }
}
