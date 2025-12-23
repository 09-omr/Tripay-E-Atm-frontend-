namespace digitalpayment3.Models
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // Deposit / Withdraw
        public string Status { get; set; } = "Success";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
    }
}
