using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Commands
{
    public class ProcessPaymentCommand
    {
        public OrderModel FromOrder { get; set; }
    }
}
