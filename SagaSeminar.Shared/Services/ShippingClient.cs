using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;
using System.Net.Http.Json;

namespace SagaSeminar.Shared.Services
{
    public class ShippingClient : IShippingClient
    {
        private readonly HttpClient _httpClient;

        public ShippingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ListResponseModel<DeliveryModel>> GetDeliveries(int skip, int take)
        {
            ListResponseModel<DeliveryModel> response = await _httpClient.GetFromJsonAsync<ListResponseModel<DeliveryModel>>($"/api/deliveries?skip={skip}&take={take}");

            return response;
        }
    }
}
