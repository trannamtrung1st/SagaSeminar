namespace SagaSeminar.Services.PaymentService.Services.Interfaces
{
    public interface IPaymentChoreographySaga
    {
        Task HandleCancelPaymentWhenDeliveryFailed(CancellationToken cancellationToken);
        Task HandleCreatePaymentWhenOrderCreated(CancellationToken cancellationToken);
        Task HandleCancelPaymentWhenInventoryDeliveryFailed(CancellationToken cancellationToken);
    }
}
