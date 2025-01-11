using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models
{
    public class Wallet
    {
        public int Id { get; set; } 
        public string UserId { get; set; }
        public decimal CurrentBalance {  get; set; }
        [EnumDataType(typeof(Currency))]
        public Currency Currency { get; set; }
    }
    public enum Currency
    {
        EUR=1,
        USD=2,
        GEL=3
    }

}
