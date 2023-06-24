using Microsoft.EntityFrameworkCore;

namespace SagaSeminar.Shared.Service.Repositories.Interfaces
{
    public interface IRepository<TDbContext, E>
        where TDbContext : DbContext
        where E : class
    {
        Task Create(E entity);
        Task Update(E entity);
        Task Delete(E entity);
        IQueryable<E> Query();
    }
}
