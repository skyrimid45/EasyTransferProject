using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Common;

namespace Project.Models
{
    public class CustomerContext:DbContext
    {
        public CustomerContext() : base("BankConnection") {
            Database.SetInitializer<CustomerContext>(null);
        }

        // Define database tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<ETransfer> ETransfers { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<ChequeUpload> ChequeUploads { get; set; }

    }
}