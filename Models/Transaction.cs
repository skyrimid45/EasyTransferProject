using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        [ForeignKey("Customer")]
        public int CustId { get; set; }

        [Required(ErrorMessage = "Account ID is required.")]
        [ForeignKey("Account")]
        public int AccountID { get; set; }

        [Required(ErrorMessage = "Transaction date is required.")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual Account Account { get; set; }
    }
}
