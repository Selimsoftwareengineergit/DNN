namespace DNN.Models.ViewModels
{
    public class EditUserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string? CurrentProfileImagePath { get; set; }
        public IFormFile? NewProfileImage { get; set; }     
        public bool DeleteExistingImage { get; set; }      
    }
}