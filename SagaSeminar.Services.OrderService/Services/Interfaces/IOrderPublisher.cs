using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.OrderService.Services.Interfaces
{
    public interface IOrderPublisher
    {
        Task PublishOrderCreated(OrderModel model);
        Task PublishOrderCompleted(Guid transactionId);
        Task PublishCompleteOrderFailed(Guid transactionId, string note);
    }
}
