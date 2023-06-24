using Microsoft.EntityFrameworkCore.Design;

namespace SagaSeminar.Services.PaymentService.Entities
{
    public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
    {
        public PaymentDbContext CreateDbContext(string[] args)
        {
            return new PaymentDbContext();
        }
    }
}
