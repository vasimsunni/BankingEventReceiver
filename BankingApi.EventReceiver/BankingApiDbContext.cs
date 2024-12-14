using Microsoft.EntityFrameworkCore;

namespace BankingApi.EventReceiver
{
    public class BankingApiDbContext : DbContext
    {
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<TransactionHistory> TransactionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BankAccount table without foreign key constraints
            modelBuilder.Entity<BankAccount>()
                .HasKey(ba => ba.Id);

            // Configure TransactionHistory table without foreign key constraints
            modelBuilder.Entity<TransactionHistory>()
                .HasKey(th => th.Id); // Id is the primary key

            // No foreign key relationships are set here for BankAccount or BankAccountTransaction
        }
    }
}
