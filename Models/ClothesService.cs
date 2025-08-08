namespace Laundry.Models
{
    public class ClothesService
    {
        public int ClothesId { get; set; }
        public Clothes Clothes { get; set; }

        public int ServicesId { get; set; }
        public Services Services { get; set; }
    }
}
