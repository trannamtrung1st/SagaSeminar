using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.Interfaces
{
    public interface IOrderProcessingPublisher
    {
        Task ProcessPayment(OrderModel order);
        Task InventoryDelivery(PaymentModel payment);
        Task ProcessDelivery(InventoryNoteModel note);
        Task CompleteOrder(DeliveryModel delivery);
        Task ReverseInventoryDelivery(Guid transactionId, string note);
        Task CancelPayment(Guid transactionId, string note);
        Task CancelOrder(Guid transactionId, string note);
        Task Retry(Guid transactionId, Guid sagaTransactionId);
    }
}
