using MvcProject.Models.IRepository.Enum;

namespace MvcProject.Models.Model
{
    public class Withdraw
    {
        public int TransactionID {  get; set; }
        public decimal Amount { get; set; }
        public string? MerchantID {  get; set; }
        public int UsersAccountNumber {  get; set; }
        public string? UsersFullName {  get; set; }
        public string? Hash { get; set; }
        public Status Status { get; set; }
    }
}
