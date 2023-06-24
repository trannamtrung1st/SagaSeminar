using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Commands
{
    public class ProcessDeliveryCommand
    {
        public InventoryNoteModel FromNote { get; set; }
    }
}
