using System.ComponentModel.DataAnnotations;

namespace DNN.Models.ViewModels
{
    public class RegistrationViewModel
    {
        [Required, StringLength(50)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        [StringLength(256)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required, Phone]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        // ✅ Add this for profile image upload
        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }
    }
}