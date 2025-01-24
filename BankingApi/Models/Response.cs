namespace BankingApi.Models
{
    public class Response
    {
        public int DepositWithdrawRequestId {  get; set; }
        public Status Status { get; set; }
        public decimal Amount {  get; set; }
    }
    
}
