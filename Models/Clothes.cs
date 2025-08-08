namespace Laundry.Models
{
    public class Clothes
    {

        public int Id { get; set; }

        public string Title { get; set; }

        public int? Quantity { get; set; }

        public string Image { get; set; }

        public DateTime Createdat { get; set; } = DateTime.Now;
       
        public List<ClothesService> ClothesServices { get; set; }
    }
}
