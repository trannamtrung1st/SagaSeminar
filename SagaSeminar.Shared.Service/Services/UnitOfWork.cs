using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Shared.Service
{
    public class UnitOfWork<TDbContext> :
        IDisposable, IUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginTransaction()
        {
            if (_currentTransaction == null)
            {
                _currentTransaction = await _dbContext.Database.BeginTransactionAsync();
            }
        }

        public async Task CommitChanges(bool isFinal = true)
        {
            await _dbContext.SaveChangesAsync();

            if (isFinal && _currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }

        public void Dispose() => _currentTransaction?.Dispose();
    }
}
