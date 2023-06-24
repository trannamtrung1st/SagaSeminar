namespace SagaSeminar.Services.PaymentService.Services.Interfaces
{
    public interface IPaymentOrchestratorSaga
    {
        Task HandleProcessPayment(CancellationToken cancellationToken);
        Task HandleCancelPayment(CancellationToken cancellationToken);
    }
}
