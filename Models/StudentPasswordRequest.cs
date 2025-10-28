using System.ComponentModel.DataAnnotations;

namespace DNN.Models
{
    public class StudentPasswordRequest    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        public string RequestType { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        public string? NewPassword { get; set; }

        public string? AdminRemarks { get; set; }
    }
}