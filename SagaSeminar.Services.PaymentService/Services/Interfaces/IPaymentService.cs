using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task CreatePaymentFromOrder(OrderModel model);
        Task CancelPayment(Guid transactionId, string note);
        Task<ListResponseModel<PaymentModel>> GetPayments(int skip, int take);
    }
}
