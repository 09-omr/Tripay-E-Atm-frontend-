namespace digitalpayment3.Models
{
    public class AccountViewModel
    {
        public int Id { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Currency { get; set; } = "TRY";
        public decimal Balance { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
