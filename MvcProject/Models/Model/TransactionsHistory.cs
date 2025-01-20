namespace MvcProject.Models.Model
{
    public class TransactionsHistory
    {
        public int Id { get; set; }
        public int TransactionId {  get; set; }
        public bool? SentToBankingApi { get; set; } = false;

    }
}
