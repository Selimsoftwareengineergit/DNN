using DNN.Controllers;
using System.ComponentModel.DataAnnotations;

namespace DNN.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(256)]
        public string PasswordHash { get; set; } // We'll store hashed password

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        [StringLength(20)]
        public string MobileNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}