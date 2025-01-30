namespace BankingApi.Helper
{
    public interface IHash
    {
        string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string userFullName, string secretKey);
        string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string secretKey);
    }
}
