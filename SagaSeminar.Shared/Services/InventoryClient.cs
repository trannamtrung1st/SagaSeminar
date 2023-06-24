using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;
using System.Net.Http.Json;

namespace SagaSeminar.Shared.Services
{
    public class InventoryClient : IInventoryClient
    {
        private readonly HttpClient _httpClient;

        public InventoryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<int> GetAvailableQuantity()
        {
            int response = await _httpClient.GetFromJsonAsync<int>($"/api/inventory-notes/available-quantity");

            return response;
        }

        public async Task<ListResponseModel<InventoryNoteModel>> GetInventoryNotes(int skip, int take)
        {
            ListResponseModel<InventoryNoteModel> response = await _httpClient.GetFromJsonAsync<ListResponseModel<InventoryNoteModel>>(
                $"/api/inventory-notes?skip={skip}&take={take}");

            return response;
        }
    }
}
