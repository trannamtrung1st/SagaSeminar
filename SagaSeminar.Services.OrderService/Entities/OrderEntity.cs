using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.OrderService.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public string Customer { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Status { get; set; }
        public Guid TransactionId { get; set; }
        public string Note { get; set; }

        public OrderModel ToOrderModel()
        {
            return new OrderModel
            {
                Id = Id,
                Amount = Amount,
                Customer = Customer,
                CreatedTime = CreatedTime,
                Status = Status,
                TransactionId = TransactionId,
                Note = Note
            };
        }
    }
}
