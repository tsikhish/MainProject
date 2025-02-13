namespace MvcProject.Models.Enum
{
    public enum CustomStatusCode
    {
        AlreadyExists = 201,
        PendingRequestExists = 400,
        InsufficientBalance = 401,
        AmountMismatch = 402,
        InvalidAmount = 403,
        NotExists = 404,
        ChangedStatus = 405,
        TransactionFailed = 406,
        InternalServerError = 500,
    }

}
