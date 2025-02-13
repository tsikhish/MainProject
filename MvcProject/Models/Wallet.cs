using MvcProject.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal CurrentBalance { get; set; }
        [EnumDataType(typeof(Currency))]
        public Currency Currency { get; set; }
    }


}
