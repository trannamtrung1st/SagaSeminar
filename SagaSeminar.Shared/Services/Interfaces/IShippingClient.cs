using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Services.Interfaces
{
    public interface IShippingClient
    {
        Task<ListResponseModel<DeliveryModel>> GetDeliveries(int skip, int take);
    }
}
