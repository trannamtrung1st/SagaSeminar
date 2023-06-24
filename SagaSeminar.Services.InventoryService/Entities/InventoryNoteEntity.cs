using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.InventoryService.Entities
{
    public class InventoryNoteEntity
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Reason { get; set; }
        public Guid TransactionId { get; set; }
        public string Note { get; set; }

        public InventoryNoteModel ToInventoryNoteModel()
        {
            return new InventoryNoteModel
            {
                Id = Id,
                Quantity = Quantity,
                CreatedTime = CreatedTime,
                Reason = Reason,
                TransactionId = TransactionId,
                Note = Note
            };
        }
    }
}
