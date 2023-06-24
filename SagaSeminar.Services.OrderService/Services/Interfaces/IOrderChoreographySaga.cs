namespace SagaSeminar.Services.OrderService.Services.Interfaces
{
    public interface IOrderChoreographySaga
    {
        Task HandleCancelOrderWhenPaymentFailed(CancellationToken cancellationToken);
        Task HandleCancelOrderWhenInventoryDeliveryFailed(CancellationToken cancellationToken);
        Task HandleCancelOrderWhenDeliveryFailed(CancellationToken cancellationToken);
        Task HandleCompleteOrderWhenDeliveryCreated(CancellationToken cancellationToken);
    }
}
