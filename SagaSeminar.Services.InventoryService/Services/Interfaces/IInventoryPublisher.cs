using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.InventoryService.Services.Interfaces
{
    public interface IInventoryPublisher
    {
        Task PublishInventoryDeliveryNoteCreated(InventoryNoteModel model);
        Task PublishInventoryDeliveryFailed(Guid transactionId, string note);
    }
}
