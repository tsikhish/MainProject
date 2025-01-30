using BankingApi.Models;

namespace BankingApi.Service
{
    public interface ISendBackResponse
    {
        Task SendWithdrawResultToMvcProject(Withdraw withdraw, Status status);
    }    
}
