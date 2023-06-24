namespace SagaSeminar.Services.InventoryService.Services.Interfaces
{
    public interface IInventoryChoreographySaga
    {
        Task HandleInventoryDeliveryWhenOrderPaymentCreated(CancellationToken cancellationToken);
        Task HandleReverseInventoryDeliveryWhenDeliveryFailed(CancellationToken cancellationToken);
    }
}
