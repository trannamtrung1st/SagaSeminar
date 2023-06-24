namespace SagaSeminar.Services.ShippingService.Services.Interfaces
{
    public interface IShippingOrchestratorSaga
    {
        Task HandleProcessDelivery(CancellationToken cancellationToken);
    }
}
