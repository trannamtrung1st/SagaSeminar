using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.InventoryService.Services.Interfaces
{
    public interface IInventoryService
    {
        Task CreateInventoryDeliveryNote(PaymentModel fromPayment);
        Task ReverseDelivery(Guid transactionId, string note);
        Task<ListResponseModel<InventoryNoteModel>> GetInventoryNotes(int skip, int take);
        Task<int> GetInventoryAvailableQuantity();
    }
}
