using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.PaymentService.Entities
{
    public class PaymentEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public double Amount { get; set; }
        public string Customer { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Status { get; set; }
        public Guid TransactionId { get; set; }
        public string Note { get; set; }

        public PaymentModel ToPaymentModel()
        {
            return new PaymentModel
            {
                Id = Id,
                OrderId = OrderId,
                Amount = Amount,
                Customer = Customer,
                Status = Status,
                CreatedTime = CreatedTime,
                TransactionId = TransactionId,
                Note = Note
            };
        }
    }
}
