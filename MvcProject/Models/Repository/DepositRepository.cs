using Azure.Core;
using Dapper;
using Microsoft.Extensions.Options;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Model.DTO;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;

namespace MvcProject.Models.Repository
{
    public class DepositRepository : IDepositRepository
    {
        private readonly IDbConnection _connection;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IHash256 _hash;
        private readonly string _secretKey;

        public DepositRepository(IOptions<AppSettings> appSettings, IHash256 hash, ITransactionRepository transactionRepository,IDbConnection connection)
        {
            _secretKey = appSettings.Value.SecretKey;
            _hash = hash;
            _transactionRepository = transactionRepository;
            _connection = connection;
        }
        public async Task<Deposit> ValidateDeposit(string userId,DepositRequestDTO request)
        {
            if (request.Amount <= 0)
                throw new Exception("Amount must be greater than 0.");

            if (string.IsNullOrEmpty(userId))
                throw new Exception("User not authenticated.");

            var depositWithdrawId = await _transactionRepository
                .RegisterDepositWithdraw(userId, Status.Pending, TransactionType.Deposit, request.Amount);

            if (depositWithdrawId == null)
                throw new Exception("Failed to register the deposit transaction.");

            var hash = _hash.ComputeSHA256Hash((int)(request.Amount * 100), userId, depositWithdrawId, _secretKey);
            return new Deposit
            {
                TransactionID = depositWithdrawId,
                MerchantID = userId,
                Amount = (int)(request.Amount * 100), // Amount in cents
                Hash = hash,
            };
        }
        public async Task<string> GetUserIdByResponse(Response response)
        {
            var query = "Select Id from DepositWithdrawRequest where Id=@id";
            var withdrawId = await _connection.QueryFirstOrDefaultAsync<int>
                (query, new { Id = response.DepositWithdrawRequestId });
            var userId = "select userId from DepositWithdrawRequest where Id=@id";
            return await _connection.QueryFirstOrDefaultAsync<string>(userId, new
            {
                Id = withdrawId
            });
        }
        public async Task<Response> SendToBankingApi(Deposit deposit, string action)
        {
            try
            {
                using var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(deposit), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"https://localhost:7133/Transactions/{action}", content);
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
