using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Common;

namespace Project.Models
{
    public class CustomerContext : DbContext
    {
        public CustomerContext() : base("BankConnection")
        {
            Database.SetInitializer<CustomerContext>(null);
        }

        // Define database tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ETransfer> ETransfers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<ChequeUpload> ChequeUploads { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }


        // override method to configure cascade delete
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Cascade delete: if a customer is deleted, their accounts are deleted too
            modelBuilder.Entity<Account>()
                .HasRequired(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
