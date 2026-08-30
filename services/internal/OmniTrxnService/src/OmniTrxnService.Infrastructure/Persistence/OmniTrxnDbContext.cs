using Microsoft.EntityFrameworkCore;
using OmniTrxnService.Domain;
using OmniTrxnService.Domain.Entities;


namespace OmniTrxnService.Infrastructure.Persistence
{
    public class OmniTrxnDbContext : DbContext
    {
        public OmniTrxnDbContext(DbContextOptions<OmniTrxnDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<VendorCustomerMap> VendorCustomerMaps => Set<VendorCustomerMap>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CustomerNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.CustomerNumber).IsUnique();
            });

            modelBuilder.Entity<VendorCustomerMap>(entity =>
            {
                entity.ToTable("Vendor_Customer_Map");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CustomerNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.VendorCustomerNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.VendorName).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(e => new { e.VendorName, e.VendorCustomerNumber }).IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(30);
                entity.Property(e => e.CustomerNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DebitCredit).HasConversion<string>().HasMaxLength(10);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Reference).HasMaxLength(255);
                entity.Property(e => e.TransDate).IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(3);
                entity.Property(e => e.Vendor).HasConversion<string>().HasMaxLength(20);
                entity.HasOne(t => t.Customer)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.Vendor, e.TransactionId }).IsUnique();
            });
        }
    }
}
