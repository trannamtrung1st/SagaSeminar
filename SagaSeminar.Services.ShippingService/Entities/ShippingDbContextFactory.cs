using Microsoft.EntityFrameworkCore.Design;

namespace SagaSeminar.Services.ShippingService.Entities
{
    public class ShippingDbContextFactory : IDesignTimeDbContextFactory<ShippingDbContext>
    {
        public ShippingDbContext CreateDbContext(string[] args)
        {
            return new ShippingDbContext();
        }
    }
}
