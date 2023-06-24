using Microsoft.EntityFrameworkCore;

namespace SagaSeminar.Services.InventoryService.Entities
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions options) : base(options)
        {
        }

        public InventoryDbContext()
        {
        }

        public virtual DbSet<InventoryNoteEntity> InventoryNote { get; set; }

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

            modelBuilder.Entity<InventoryNoteEntity>(builder =>
            {
                builder.HasData(
                    new InventoryNoteEntity
                    {
                        Id = Guid.NewGuid(),
                        CreatedTime = DateTime.Now,
                        Quantity = 1000000,
                        Reason = "Initial reception",
                        TransactionId = Guid.NewGuid()
                    });
            });
        }
    }
}
