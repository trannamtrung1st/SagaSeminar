using Microsoft.EntityFrameworkCore;

namespace SagaSeminar.Shared.Service.Services.Interfaces
{
    public interface IUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        Task BeginTransaction();
        Task CommitChanges(bool isFinal = true);
    }
}
