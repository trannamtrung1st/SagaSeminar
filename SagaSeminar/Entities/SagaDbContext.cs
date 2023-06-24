using Microsoft.EntityFrameworkCore;

namespace SagaSeminar.Entities
{
    public class SagaDbContext : DbContext
    {
        public SagaDbContext(DbContextOptions options) : base(options)
        {
        }

        public SagaDbContext()
        {
        }

        public virtual DbSet<TransactionEntity> Transaction { get; set; }
        public virtual DbSet<SagaTransactionEntity> SagaTransaction { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransactionEntity>(builder =>
            {
                builder.HasOne(e => e.RunningTransaction)
                    .WithMany()
                    .HasForeignKey(e => e.RunningTransactionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SagaTransactionEntity>(builder =>
            {
                builder.Property(e => e.Id).ValueGeneratedOnAdd();

                builder.HasOne(e => e.Transaction)
                    .WithMany(e => e.SagaTransactions)
                    .HasForeignKey(e => e.TransactionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
