using System.ComponentModel.DataAnnotations;

namespace Laundry.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
        public _Role Role { get; set; } = _Role.User;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Booking>? Bookings { get; set; }

        public enum _Role
        {
            Admin, User
        }
    }
}
