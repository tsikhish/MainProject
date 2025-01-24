namespace MvcProject.Models.Hash
{
    public interface IHash256
    {
        string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string secretKey);
        string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string usersFullName, string secretKey);
    }

}
