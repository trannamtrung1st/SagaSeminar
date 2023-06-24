namespace SagaSeminar.Shared.Models
{
    public class InventoryNoteModel
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Reason { get; set; }
        public Guid TransactionId { get; set; }
        public string Note { get; set; }
    }
}
