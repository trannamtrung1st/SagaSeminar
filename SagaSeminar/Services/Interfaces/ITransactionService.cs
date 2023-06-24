using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.Interfaces
{
    public interface ITransactionService
    {
        Task UpdateTransaction(Guid id, string status, params SagaTransactionUpdateModel[] sagaTransactions);
        Task<ListResponseModel<TransactionListingModel>> GetTransactions(int skip, int take);
        Task<TransactionDetailsModel> GetTransactionDetails(Guid transactionId);
    }
}
