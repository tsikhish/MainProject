using Microsoft.AspNetCore.Mvc;

namespace MvcProject.Models.Exceptions
{
    public class WithdrawException : Exception
    {
        public int ErrorCode { get; }

        public WithdrawException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }

}
