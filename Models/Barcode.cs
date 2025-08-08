namespace Laundry.Models
{
    public class Barcode
    {
        public int Id { get; set; }
        public int BookingClothesId { get; set; }
        public string BarcodeValue { get; set; }
        public string BarcodeImagePath { get; set; }

        public BookingClothes? BookingClothes { get; set; }
    }
}
