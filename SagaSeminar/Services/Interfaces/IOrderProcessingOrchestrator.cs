namespace SagaSeminar.Services.Interfaces
{
    public interface IOrderProcessingOrchestrator
    {
        Task Start(CancellationToken cancellationToken);
    }
}
