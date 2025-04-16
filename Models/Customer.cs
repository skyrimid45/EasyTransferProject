using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int CustId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Telephone is required.")]
        [StringLength(15, ErrorMessage = "Telephone cannot exceed 15 characters.")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Invalid telephone number.")]
        public string Telephone { get; set; }

        public string Role { get; set; }

        // navigation property for cascade delete
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
