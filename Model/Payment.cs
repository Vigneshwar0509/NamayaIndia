using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Namaya.Model
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Verification"; // Default to Verification
        public string PaymentMethod { get; set; } // "Cash" or "UPI"
        public string? ReceiptPath { get; set; } // Nullable for Cash
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public DateTime? VerifiedDate { get; set; }
    }
}