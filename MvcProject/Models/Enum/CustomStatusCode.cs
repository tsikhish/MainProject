namespace MvcProject.Models.Enum
{
    public enum CustomStatusCode
    {
        AlreadyExists = 201,
        PendingRequestExists = 400,
        InsufficientBalance = 401,
        AmountMismatch = 402,
        NotExists = 404,
        ChangedStatus = 405,
        InternalServerError = 500,

    }

}
