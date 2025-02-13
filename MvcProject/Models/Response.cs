using MvcProject.Models.Enum;

namespace MvcProject.Models
{
    public class Response
    {
        public int DepositWithdrawRequestId { get; set; }
        public Status Status { get; set; }
        public string? PaymentUrl { get; set; }
        public decimal Amount { get; set; }
    }
}
