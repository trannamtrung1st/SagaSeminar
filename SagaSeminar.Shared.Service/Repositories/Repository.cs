using Microsoft.EntityFrameworkCore;
using SagaSeminar.Shared.Service.Repositories.Interfaces;

namespace SagaSeminar.Shared.Service.Repositories
{
    public class Repository<TDbContext, E> : IRepository<TDbContext, E>
        where TDbContext : DbContext
        where E : class
    {
        protected readonly TDbContext dbContext;

        public Repository(TDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Create(E entity)
        {
            await dbContext.AddAsync(entity);
        }

        public Task Delete(E entity)
        {
            dbContext.Remove(entity);

            return Task.CompletedTask;
        }

        public IQueryable<E> Query()
        {
            return dbContext.Set<E>();
        }

        public Task Update(E entity)
        {
            dbContext.Update(entity);

            return Task.CompletedTask;
        }
    }
}
