﻿using System.Security.Cryptography;
using System.Text;

namespace BankingApi.Models.Hash
{
    public static class Hash256
    {
        public static string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string secretKey)
        {
            string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public static string ComputeSHA256Hash(int amount, string merchantId, int transactionId, string usersFullName, string secretKey)
        {
            string concatenatedData = $"{amount}+{merchantId}+{transactionId}+{usersFullName}+{secretKey}";
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
}
