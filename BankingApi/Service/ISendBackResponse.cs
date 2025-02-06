using BankingApi.Models;

namespace BankingApi.Service
{
    public interface ISendBackResponse
    {
        Task SendDepositResultToMvcProject(Deposit deposit,Status status);
        Task SendWithdrawResultToMvcProject(Withdraw withdraw, Status status);
    }    
}
