using Microsoft.EntityFrameworkCore;

namespace SagaSeminar.Services.ShippingService.Entities
{
    public class ShippingDbContext : DbContext
    {
        public ShippingDbContext(DbContextOptions options) : base(options)
        {
        }

        public ShippingDbContext()
        {
        }

        public virtual DbSet<DeliveryEntity> Delivery { get; set; }

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
        }
    }
}
