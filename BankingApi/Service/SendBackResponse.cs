using BankingApi.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace BankingApi.Service
{
    public class SendBackResponse: ISendBackResponse
    {
        private readonly string _apiUrl;
        public SendBackResponse(IOptions<AppSettings> appSettings)
        {
            _apiUrl = appSettings.Value.ApiUrl;
        }
        public async Task SendWithdrawResultToMvcProject(Withdraw withdraw, Status status)
        {
            try
            {
                using var client = new HttpClient();
                var request = new
                {
                    DepositWithdrawRequestId = withdraw.TransactionID,
                    Amount = withdraw.Amount,
                    Status = status
                };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiUrl}/SuccessWithdraw", content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to notify MVC project about the transaction result.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
