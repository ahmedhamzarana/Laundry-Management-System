namespace Laundry.Models
{
    public class BookingService
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int ServiceId { get; set; }
        public Services Service { get; set; }
    }

}
