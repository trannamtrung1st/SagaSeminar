namespace SagaSeminar.Services.ShippingService.Services.Interfaces
{
    public interface IShippingChoreographySaga
    {
        Task HandleCreateDeliveryWhenInventoryGoodsDelivered(CancellationToken cancellationToken);
    }
}
