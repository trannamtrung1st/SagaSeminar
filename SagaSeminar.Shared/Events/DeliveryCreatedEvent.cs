using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Events
{
    public class DeliveryCreatedEvent
    {
        public DeliveryModel Model { get; set; }
    }
}
