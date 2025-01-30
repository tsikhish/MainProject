using System.Security.Cryptography;
using System.Text;

namespace BankingApi.Helper
{
    public class Hash: IHash
    {
        public string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string userFullName, string secretKey)
        {
            string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{userFullName}+{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string secretKey)
        {
            string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
}
