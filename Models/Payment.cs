using System.ComponentModel.DataAnnotations.Schema;

namespace Laundry.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public Method PaymentMethod { get; set; }  // Cash, Card, etc.
        public Status PaymentStatus { get; set; }  // Pending, Completed, Failed
        public string? TransactionId { get; set; }

        public enum Method
        {
            Cash, Card
        }

        public enum Status
        {
            Pending, Completed, Failed
        }
    }

}
