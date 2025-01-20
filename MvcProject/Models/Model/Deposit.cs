using MvcProject.Models.IRepository.Enum;

namespace MvcProject.Models.Model
{
    public class Deposit
    {
        public int DepositWithdrawId { get; set; }
        public Guid TransactionID { get; set; }
        public decimal Amount { get; set; }
        public string? MerchantID { get; set; }
        public string? Hash { get; set; }
        public Status Status { get; set; }
    }
}
