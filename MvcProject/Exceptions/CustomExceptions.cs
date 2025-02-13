using log4net;
using MvcProject.Models.Enum;
namespace MvcProject.Exceptions
{
    public class CustomException : Exception
    {
        public CustomStatusCode StatusCode { get; }

        public CustomException(CustomStatusCode statusCode)
            : base($"Error occurred: {statusCode}")
        {
            StatusCode = statusCode;
        }

        public CustomException(CustomStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
