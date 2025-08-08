using System.ComponentModel.DataAnnotations;

namespace Laundry.Models
{
    public class Customer
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        [StringLength(60)]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }

        [Required]
        public string Phone { get; set; }


        public string? Email { get; set; }


        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}
