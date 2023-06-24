using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.ShippingService.Services.Interfaces
{
    public interface IShippingService
    {
        Task CreateDelivery(InventoryNoteModel fromNote);
        Task<ListResponseModel<DeliveryModel>> GetDeliveries(int skip, int take);
    }
}
