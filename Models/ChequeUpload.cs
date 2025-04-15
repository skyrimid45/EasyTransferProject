using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("ChequeUploads")]
    public class ChequeUpload
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustId { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        public string AccountType { get; set; } // "Chequings" or "Savings"

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public virtual Customer Customer { get; set; }
    }
}

