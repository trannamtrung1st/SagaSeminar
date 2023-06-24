using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.ShippingService.Entities
{
    public class DeliveryEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Customer { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedTime { get; set; }
        public Guid TransactionId { get; set; }

        public DeliveryModel ToDeliveryModel()
        {
            return new DeliveryModel
            {
                Id = Id,
                OrderId = OrderId,
                Customer = Customer,
                Quantity = Quantity,
                CreatedTime = CreatedTime,
                TransactionId = TransactionId
            };
        }
    }
}
