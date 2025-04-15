using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Project.Models
{
    public enum AccountType
    {
        Chequings = 0,
        Savings = 1
    }

    [Table("Account")]
    public class Account
    {
        [Key]
        public int AccountID { get; set; }

        [Required]
        public int AccountNum { get; set; }

        [Required]
        public AccountType Type { get; set; } 

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal Balance { get; set; }

        public decimal GiftBalance { get; set; }

        [ForeignKey("Customer")]
        public int? CustId { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
