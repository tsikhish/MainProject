namespace MvcProject.Models.Exceptions
{
    public interface ICustomExceptions
    {
        Task WithdrawExceptions(int outputParam2Value, string userId);
        Task TransactionWithdrawException(int returnCode,string userId);
        Task DepositException(int depositId, string userId, int outputParam2Value);

    }
}
