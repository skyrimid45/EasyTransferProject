using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Project.Models
{
    public class TransactionContext : DbContext
    {
        public TransactionContext() : base("BankConnection")
        {
            Database.SetInitializer<TransactionContext>(null);
        }

        // Define the Transactions table
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }
}
