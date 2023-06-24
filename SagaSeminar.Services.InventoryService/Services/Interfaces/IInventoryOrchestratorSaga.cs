namespace SagaSeminar.Services.InventoryService.Services.Interfaces
{
    public interface IInventoryOrchestratorSaga
    {
        Task HandleInventoryDelivery(CancellationToken cancellationToken);
        Task HandleReverseInventoryDelivery(CancellationToken cancellationToken);
    }
}
