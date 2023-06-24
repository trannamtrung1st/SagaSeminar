using Microsoft.EntityFrameworkCore.Design;

namespace SagaSeminar.Services.InventoryService.Entities
{
    public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        public InventoryDbContext CreateDbContext(string[] args)
        {
            return new InventoryDbContext();
        }
    }
}
