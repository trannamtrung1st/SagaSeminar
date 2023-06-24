using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Events
{
    public class InventoryDeliveryNoteCreatedEvent
    {
        public InventoryNoteModel Model { get; set; }
    }
}
