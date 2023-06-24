namespace SagaSeminar.Shared.Models
{
    public class DeliveryModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public string Customer { get; set; }
        public DateTime CreatedTime { get; set; }
        public Guid TransactionId { get; set; }
    }
}
