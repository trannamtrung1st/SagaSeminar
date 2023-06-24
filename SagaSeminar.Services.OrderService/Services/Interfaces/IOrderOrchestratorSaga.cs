namespace SagaSeminar.Services.OrderService.Services.Interfaces
{
    public interface IOrderOrchestratorSaga
    {
        Task HandleCancelOrder(CancellationToken cancellationToken);
        Task HandleCompleteOrder(CancellationToken cancellationToken);
    }
}
