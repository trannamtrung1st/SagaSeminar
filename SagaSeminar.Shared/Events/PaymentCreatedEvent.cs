using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Events
{
    public class PaymentCreatedEvent
    {
        public PaymentModel Model { get; set; }
    }
}
