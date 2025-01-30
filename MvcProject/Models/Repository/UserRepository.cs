using Azure.Core;
using Microsoft.Extensions.Options;
using MvcProject.Models.Model;
using MvcProject.Models.Repository.IRepository;
using Newtonsoft.Json;
using System.Text;

namespace MvcProject.Models.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _merchantId;
        private readonly string _casinoUrl;
        public UserRepository(IOptions<AppSettings> appSettings)
        {
            _merchantId = appSettings.Value.MerchantID;
            _casinoUrl = appSettings.Value.CasinoUrl;
        }
        public async Task<string> SendPublicToken(string userId)
        {
            var publicToken = new Token { PublicToken = Guid.NewGuid(),PrivateToken=Guid.NewGuid() };

            var parameters = new
            {
                PrivateToken = publicToken.PrivateToken,
                PublicToken = publicToken.PublicToken,
                UserId = userId,
                MerchantId = _merchantId,
                Lang = "ENG",
            };
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_casinoUrl}", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            else
            {
                return "Error generating private token";
            }
        }
    }
}
