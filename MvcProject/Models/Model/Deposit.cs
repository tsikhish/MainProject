using Newtonsoft.Json;

namespace MvcProject.Models.Model
{
    public class Deposit
    {
        public int TransactionID { get; set; }
        public decimal Amount { get; set; }
        public string? MerchantID { get; set; }
        public string? Hash { get; set; }
    }
}
