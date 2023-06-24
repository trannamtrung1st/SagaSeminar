using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Services.Interfaces
{
    public interface IInventoryClient
    {
        Task<ListResponseModel<InventoryNoteModel>> GetInventoryNotes(int skip, int take);
        Task<int> GetAvailableQuantity();
    }
}
