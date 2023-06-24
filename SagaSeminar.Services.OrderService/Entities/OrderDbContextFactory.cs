using Microsoft.EntityFrameworkCore.Design;

namespace SagaSeminar.Services.OrderService.Entities
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            return new OrderDbContext();
        }
    }
}
