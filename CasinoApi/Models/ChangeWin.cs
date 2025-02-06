namespace CasinoApi.Models
{
    public class ChangeWin
    {
        public string PrivateToken { get; set; }
        public long Amount { get; set; }
        public long PreviousAmount { get; set; }
        public string TransactionId { get; set; }
        public string PreviousTransactionId { get; set; }
        public int ChangeWinType {  get; set; }
        public int GameId { get; set; }
        public int ProductId { get; set; }
        public int RoundId { get; set; }
        public string? Hash { get; set; }
        public string? Currency { get; set; }
    }
}
