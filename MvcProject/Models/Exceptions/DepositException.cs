namespace MvcProject.Models.Exceptions
{
    public class DepositException:Exception
    {
        public int ErrorCode { get; }

        public DepositException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
