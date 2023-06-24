using Microsoft.EntityFrameworkCore.Design;

namespace SagaSeminar.Entities
{
    public class SagaDbContextFactory : IDesignTimeDbContextFactory<SagaDbContext>
    {
        public SagaDbContext CreateDbContext(string[] args)
        {
            return new SagaDbContext();
        }
    }
}
