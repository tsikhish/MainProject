using MvcProject.Models;

namespace MvcProject.Service.IService
{
    public interface IBankingRequestService
    {
        Task<Response> SendDepositToBankingApi(DepositWithdrawRequest amount, string action);
        Task<Response> SendDepositToBankingApi(int depositId, decimal amount, string action);
        Task<Response> SendWithdrawToBankingApi(Withdraw withdraw);
    }
}
