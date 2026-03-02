using System.ComponentModel.DataAnnotations;

namespace BizSecureDemo22180044.ViewModels
{
    public class CreateOrderVm
    {
        [Required, MaxLength(80)]
        public string Title { get; set; } = "";

        [Required]
        public decimal Amount { get; set; }
    }
}