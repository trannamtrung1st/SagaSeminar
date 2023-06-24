using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Commands
{
    public class InventoryDeliveryCommand
    {
        public PaymentModel FromPayment { get; set; }
    }
}
