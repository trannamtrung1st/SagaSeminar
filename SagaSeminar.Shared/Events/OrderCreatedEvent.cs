using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Events
{
    public class OrderCreatedEvent
    {
        public OrderModel Model { get; set; }
    }
}
