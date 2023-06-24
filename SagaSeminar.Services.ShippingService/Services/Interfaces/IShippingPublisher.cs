using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.ShippingService.Services.Interfaces
{
    public interface IShippingPublisher
    {
        Task PublishDeliveryCreated(DeliveryModel model);
        Task PublishDeliveryFailed(Guid transactionId, string note);
    }
}
