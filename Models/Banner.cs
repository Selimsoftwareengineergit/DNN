using DNN.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace DNN.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

 
        [StringLength(1000)]
        public string? ImagePath { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(5000)]
        public string? ClickUrl { get; set; }

        [StringLength(20)]
        public string Target { get; set; } = "_blank";

        [Required]
        public BannerTypeEnum BannerType { get; set; } = BannerTypeEnum.Slider; 

        public int Priority { get; set; } = 1;

        public int Impressions { get; set; } = 0;

        public int Clicks { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }
    }
}