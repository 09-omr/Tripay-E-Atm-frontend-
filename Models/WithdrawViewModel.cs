using System.ComponentModel.DataAnnotations;

namespace digitalpayment3.Models
{
    public class WithdrawViewModel
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Tutar gereklidir")]
        [Range(1, 1000000, ErrorMessage = "Tutar 1 ile 1.000.000 arasýnda olmalýdýr")]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}
