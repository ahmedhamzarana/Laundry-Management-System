namespace Laundry.Models
{
    public class ClothesViewModel
    {
        public string Title { get; set; }
        public double Weight { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }

        public List<int>? SelectedServiceIds { get; set; }
    }
}
