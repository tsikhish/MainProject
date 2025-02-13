using Microsoft.AspNetCore.Mvc;
using MvcProject.Models;
using MvcProject.Models.DTO;
using System.Threading.Tasks;

namespace MvcProject.Service.IService
{
    public interface ITransactionService
    {
        Task<Models.Response> SendingDepositToBankingApi(DepositWithdrawRequest depositWithdrawRequest);
        Task<IEnumerable<Transactions>> TransactionHistory(string userId);
        Task<DepositWithdrawRequest> GetDepositWithdraw(int depositWithdrawId, int amount);
        Task<IEnumerable<DepositWithdrawRequest>> AdminDashboard();
        Task UpdateRejectedStatus(int id);
    }
}
