using MvcProject.Models.Model;
using MvcProject.Models.Model.DTO;

namespace MvcProject.Models.Repository.IRepository
{
    public interface IDepositRepository
    {
        Task<string> GetUserIdByResponse(Response response);
        Task<Response> SendToBankingApi(DepositWithdrawRequest deposit, string action);
        Task<DepositWithdrawRequest> ValidateDeposit(string userId, DepositRequestDTO request);
    }
}