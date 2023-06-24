using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Services.Interfaces
{
    public interface IGlobalClient
    {
        Task<GlobalConfig> GetGlobalConfig();
        Task UpdateGlobalConfig(GlobalConfig config);
        Task<ListResponseModel<TransactionListingModel>> GetTransactions(int skip, int take);
        Task<TransactionDetailsModel> GetTransactionDetails(Guid transactionId);
        Task RetryOrderTransaction(Guid transactionId, Guid sagaTransactionId);
    }
}
