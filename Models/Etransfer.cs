using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("ETransfers")]
    public class ETransfer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Sender ID is required.")]
        [ForeignKey("Sender")]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "Recipient email is required.")]
        [EmailAddress]
        public string RecipientEmail { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 10000, ErrorMessage = "Amount must be between $0.01 and $10,000.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Security question is required.")]
        public string SecurityQuestion { get; set; }

        [Required(ErrorMessage = "Security answer is required.")]
        public string SecurityAnswer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string TransferType { get; set; }

        public DateTime TransferDate { get; set; }
        public string Status { get; set; }

        // Navigation property
        public virtual Customer Sender { get; set; }
    }
}
