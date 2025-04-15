using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Project.Models
{
    public class AccountContext : DbContext
    {
        public AccountContext() : base("BankConnection")
        {
            Database.SetInitializer<AccountContext>(null);
        }

        // Define the Accounts table
        public DbSet<Account> Accounts { get; set; }

        // Define the Customers table for relationships
        public DbSet<Customer> Customers { get; set; }
    }
}