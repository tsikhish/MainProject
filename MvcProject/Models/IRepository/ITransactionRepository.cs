using MvcProject.Models.IRepository.Enum;
using MvcProject.Models.Model;

namespace MvcProject.Models.IRepository
{
    public interface ITransactionRepository
    {
        Task RegisterRejectedTransactionInTransactionsAsync(string userId, Response response);
        Task<string> GetUserIdByResponse(Response response);
        Task UpdateWithdrawStatus(int id,Status status);
        Task<int> RegisterTransactionInDepositTableAsync(string userId, Status status, TransactionType transactionType, decimal amount);
        Task RegisterSuccessTransactionInTransactionsAsync(Deposit deposit);
        Task UpdateRejectWithdraw(int id);
        Task<DepositWithdrawRequest> FindWithdraw(int id);
        Task UpdateSuccessWithdrawTable(Response response);
        Task UpdateWalletAmount(string userID, Response response);
        Task UpdateWalletAmount(Deposit deposit);
        Task<DepositWithdrawRequest> GetDepositWithdrawById(int id);
        Task<Response> SendToBankingApi(Deposit transaction,string action);
        Task<Response> SendWithdrawToBankingApi(Withdraw withdraw);
        Task UpdateSuccessDepositTable(Deposit deposit);
        Task<string> GetFullUsername(string userId);
        Task<IEnumerable<DepositWithdrawRequest>> GetTransactionByUserId(string userId);
        Task<IEnumerable<DepositWithdrawRequest>> GetWithdrawTransactionsForAdmins();
    }
}
