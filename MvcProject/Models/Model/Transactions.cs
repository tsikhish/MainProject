using MvcProject.Models.Repository.IRepository.Enum;

namespace MvcProject.Models.Model
{
    public class Transactions
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public decimal Amount { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TransactionType TransactionType { get; set; }
    }
}
