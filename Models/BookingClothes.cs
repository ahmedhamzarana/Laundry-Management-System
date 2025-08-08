namespace Laundry.Models
{
    public class BookingClothes
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }

        public int ClothesId { get; set; }
        public Clothes Clothes { get; set; }

        public int ServicesId { get; set; }
        public Services Services { get; set; }
        public int Status { get; set; } = 0;

        public Barcode? Barcode { get; set; }

    }

}
