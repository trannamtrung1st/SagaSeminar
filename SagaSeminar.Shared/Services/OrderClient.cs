using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;
using System.Net.Http.Json;

namespace SagaSeminar.Shared.Services
{
    public class OrderClient : IOrderClient
    {
        private readonly HttpClient _httpClient;

        public OrderClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateOrder(CreateOrderModel model)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/orders", model);

            response.EnsureSuccessStatusCode();
        }

        public async Task<OrderModel> GetOrderDetails(Guid? id, Guid? transactionId)
        {
            List<string> query = new List<string>();

            if (id.HasValue)
            {
                query.Add($"id={id}");
            }
            else if (transactionId.HasValue)
            {
                query.Add($"transactionId={transactionId}");
            }
            else
            {
                throw new ArgumentException();
            }

            OrderModel response = await _httpClient.GetFromJsonAsync<OrderModel>(
                $"/api/orders/details?{string.Join('&', query)}");

            return response;
        }

        public async Task<ListResponseModel<OrderModel>> GetOrders(int skip, int take)
        {
            ListResponseModel<OrderModel> response = await _httpClient.GetFromJsonAsync<ListResponseModel<OrderModel>>($"/api/orders?skip={skip}&take={take}");

            return response;
        }
    }
}
