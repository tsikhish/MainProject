using MvcProject.Models.Repository.IRepository.Enum;

namespace MvcProject.Models.Model
{
    public class Response
    {
        public int DepositWithdrawRequestId {  get; set; }
        public Status Status { get; set; }
        public string? PaymentUrl { get; set; }
        public decimal Amount { get; set; }
    }
}
