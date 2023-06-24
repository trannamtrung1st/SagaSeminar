using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Services.Interfaces
{
    public interface IPaymentClient
    {
        Task<ListResponseModel<PaymentModel>> GetPayments(int skip, int take);
    }
}
