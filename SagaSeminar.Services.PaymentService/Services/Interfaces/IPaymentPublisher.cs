using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.PaymentService.Services.Interfaces
{
    public interface IPaymentPublisher
    {
        Task PublishPaymentCreated(PaymentModel model);
        Task PublishPaymentFailed(Guid transactionId, string note);
    }
}
