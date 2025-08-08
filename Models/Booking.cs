namespace Laundry.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public _Status Status { get; set; } = _Status.Pending;
        public User? User { get; set; }
        public List<BookingClothes>? BookingClothes { get; set; }
        public List<BookingService>? BookingServices { get; set; }

        public enum _Status
        {
            Pending, Ready, Delivered
        }
        public Payment? Payment { get; set; }
    }
}
