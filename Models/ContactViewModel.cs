using System.ComponentModel.DataAnnotations;

namespace digitalpayment3.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mesaj gereklidir")]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;
    }
}
