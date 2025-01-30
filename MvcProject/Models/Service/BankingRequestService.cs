using Microsoft.Extensions.Options;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using Newtonsoft.Json;
using System.Data;
using System.Security.Policy;
using System.Text;

namespace MvcProject.Models.Service
{
    public class BankingRequestService :IBankingRequestService
    {
        private readonly IHash256 _hash;
        private readonly string _secretKey;
        private readonly string _merchantId;
        private readonly string _ApiUrl;
        public BankingRequestService(IOptions<AppSettings> appSettings, IHash256 hash)
        {
            _merchantId = appSettings.Value.MerchantID;
            _ApiUrl = appSettings.Value.ApiUrl;
            _secretKey = appSettings.Value.SecretKey;
            _hash = hash;
        }
        public async Task<Response> SendDepositToBankingApi(DepositWithdrawRequest deposit, string action)
        {
            try
            {
                var hash = _hash.ComputeSHA256Hash((int)(deposit.Amount * 100),
                    _merchantId, deposit.Id, _secretKey);
                var request = new
                {
                    TransactionID = deposit.Id,
                    MerchantID = _merchantId,
                    Amount = deposit.Amount * 100,
                    Hash = hash,
                };
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_ApiUrl}/{action}", content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Banking API returned error:{body}, {response.StatusCode} - {response.ReasonPhrase}");
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(responseBody);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize Banking API response.");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Transaction failed: " + ex.Message);
            }
        }

        public async Task<Response> SendDepositToBankingApi(int depositId,decimal amount, string action)
        {
            try
            {
                var hash = _hash.ComputeSHA256Hash((int)(amount * 100),
                    _merchantId, depositId, _secretKey);
                var request = new
                {
                    TransactionID = depositId,
                    MerchantID = _merchantId,
                    Amount = amount * 100,
                    Hash = hash,
                };
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_ApiUrl}/{action}", content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Banking API returned error:{body}, {response.StatusCode} - {response.ReasonPhrase}");
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(responseBody);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize Banking API response.");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Transaction failed: " + ex.Message);
            }
        }
        public async Task<Response> SendWithdrawToBankingApi(Withdraw withdraw)
        {
            try
            {
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(withdraw), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://localhost:7133/Transactions/ConfirmWithdraw", content);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Banking API returned error:{body}, {response.StatusCode} - {response.ReasonPhrase}");
                }
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(responseBody);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize Banking API response.");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Transaction failed: " + ex.Message);
            }

        }

    }
}
