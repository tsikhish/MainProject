namespace CasinoApi.Models
{
    public class Response
    {
        public decimal CurrentBalance { get; set; }
        public int StatusCode { get; set; }
        public string TransactionId {  get; set; }
    }
}
