using Microsoft.EntityFrameworkCore;
using SagaSeminar.Entities;
using SagaSeminar.Services.Interfaces;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IRepository<SagaDbContext, TransactionEntity> _transactionRepository;
        private readonly IUnitOfWork<SagaDbContext> _unitOfWork;

        public TransactionService(IRepository<SagaDbContext, TransactionEntity> transactionRepository,
            IUnitOfWork<SagaDbContext> unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TransactionDetailsModel> GetTransactionDetails(Guid transactionId)
        {
            TransactionDetailsModel model = await _transactionRepository.Query()
                .Where(e => e.Id == transactionId)
                .Select(e => new TransactionDetailsModel
                {
                    Id = e.Id,
                    LastUpdatedTime = e.LastUpdatedTime,
                    StartedTime = e.StartedTime,
                    Status = e.Status,
                    Transactions = e.SagaTransactions.OrderByDescending(t => t.StartedTime).Select(t => new SagaTransactionModel
                    {
                        Id = t.Id,
                        LastUpdatedTime = t.LastUpdatedTime,
                        StartedTime = t.StartedTime,
                        Status = t.Status,
                        Response = t.Response,
                        Retryable = t.Retryable,
                        Name = t.Name,
                        Note = t.Note,
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (model == null) throw new Exception("Entity not found!");

            return model;
        }

        public async Task<ListResponseModel<TransactionListingModel>> GetTransactions(int skip, int take)
        {
            IQueryable<TransactionEntity> query = _transactionRepository.Query();

            int count = await query.CountAsync();

            query = query
                .OrderByDescending(e => e.StartedTime)
                .Skip(skip).Take(take);

            TransactionListingModel[] list = await query
                .Select(e => new TransactionListingModel
                {
                    Id = e.Id,
                    LastUpdatedTime = e.LastUpdatedTime,
                    StartedTime = e.StartedTime,
                    Status = e.Status,
                    RunningTransactionName = e.RunningTransaction.Name
                }).ToArrayAsync();

            return new ListResponseModel<TransactionListingModel>
            {
                Total = count,
                List = list
            };
        }

        public async Task UpdateTransaction(Guid id, string status, params SagaTransactionUpdateModel[] sagaTransactions)
        {
            await _unitOfWork.BeginTransaction();

            TransactionEntity entity = await _transactionRepository.Query()
                .Include(e => e.SagaTransactions)
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                entity = new TransactionEntity
                {
                    Id = id,
                    Status = status,
                    StartedTime = DateTime.Now,
                    LastUpdatedTime = DateTime.Now,
                    SagaTransactions = new List<SagaTransactionEntity>()
                };

                await _transactionRepository.Create(entity);
            }
            else
            {
                entity.Status = status;
                entity.LastUpdatedTime = DateTime.Now;
            }

            foreach (SagaTransactionUpdateModel transaction in sagaTransactions)
            {
                SagaTransactionEntity existingTransaction = entity.SagaTransactions.FirstOrDefault(t => t.Name == transaction.Name);

                if (existingTransaction == null)
                {
                    SagaTransactionEntity newTransaction = new SagaTransactionEntity
                    {
                        LastUpdatedTime = DateTime.Now,
                        Name = transaction.Name,
                        Status = transaction.Status,
                        Retryable = transaction.Retryable,
                        Response = transaction.Response,
                        MessagePayload = transaction.MessagePayload,
                        Note = transaction.Note,
                        StartedTime = DateTime.Now
                    };

                    entity.SagaTransactions.Add(newTransaction);
                }
                else
                {
                    existingTransaction.Status = transaction.Status;
                    existingTransaction.Note = transaction.Note;
                    existingTransaction.LastUpdatedTime = DateTime.Now;
                    existingTransaction.Response = transaction.Response;
                }
            }

            await _unitOfWork.CommitChanges(isFinal: false);

            entity.RunningTransactionId = entity.SagaTransactions
                .OrderByDescending(e => e.StartedTime)
                .Select(e => e.Id)
                .FirstOrDefault();

            await _unitOfWork.CommitChanges();
        }
    }
}
