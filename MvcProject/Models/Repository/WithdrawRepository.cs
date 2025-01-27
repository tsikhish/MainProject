using Microsoft.Extensions.Options;
using MvcProject.Models.Hash;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using MvcProject.Models.Repository.IRepository.Enum;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Text;
using System.Transactions;

namespace MvcProject.Models.Repository
{
    public class WithdrawRepository :IWithdrawRepository
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDepositRepository _depositRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly string _secretKey;
        private readonly string _merchantId;
        private readonly IHash256 _hash;

        public WithdrawRepository(IHash256 hash,IOptions<AppSettings> appSettings, ITransactionRepository transactionRepository,
            IDepositRepository depositRepository,IWalletRepository walletRepository)
        {
            _hash = hash;
            _merchantId = appSettings.Value.MerchantID;
            _secretKey = appSettings.Value.SecretKey;
            _transactionRepository = transactionRepository;
            _depositRepository = depositRepository;
            _walletRepository = walletRepository;
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

        public async Task<Response> GetResponse(Response response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response), "Response cannot be null.");
            }

            if (!Enum.IsDefined(typeof(Status), response.Status))
            {
                throw new ArgumentException("Invalid transaction status.", nameof(response.Status));
            }
            var userId = await _depositRepository.GetUserIdByResponse(response);
            response.Amount = (decimal)(response.Amount / 100m);
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _transactionRepository.RegisterTransactionInTransactionsAsync(userId, response);
                await _transactionRepository.UpdateStatus(response.DepositWithdrawRequestId, response.Status);

                if (response.Status == Status.Success)
                {
                    await _walletRepository.UpdateWalletAmount(userId, response);
                }

                transaction.Complete();
            }
            return response;
        }
        public async Task<Withdraw> GetWithdrawTransaction(int id)
        {
            var withdraw = await _transactionRepository.GetDepositWithdrawById(id);
            if (withdraw == null) throw new Exception("Withdraw Not Found");
            var usersFullName = await _transactionRepository.GetFullUsername(withdraw.UserId);
            var hash = _hash.ComputeSHA256Hash((int)(withdraw.Amount * 100), _merchantId, withdraw.Id, usersFullName, _secretKey);
            var transaction = new Withdraw
            {
                TransactionID = id,
                MerchantID = _merchantId,
                Amount = (int)(withdraw.Amount * 100),
                Hash = hash,
                UsersFullName = usersFullName
            };
            return transaction;
        }

    }
}
