namespace BankingApi.Models
{
    public class Deposit
    {
        public int TransactionID { get; set; }
        public decimal Amount { get; set; } 
        public string? MerchantID { get; set; }
        public string? Hash { get; set; } 
        public Status Status { get; set; } 
    }
    public enum Status
    {
        Pending=1,
        Rejected=2,
        Success=3,
    }
}
