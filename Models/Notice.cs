using System;
using System.ComponentModel.DataAnnotations;

namespace DNN.Models
{
    public class Notice
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Subject { get; set; }

        public string Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? ExpireDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}