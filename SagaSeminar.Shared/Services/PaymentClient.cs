using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;
using System.Net.Http.Json;

namespace SagaSeminar.Shared.Services
{
    public class PaymentClient : IPaymentClient
    {
        private readonly HttpClient _httpClient;

        public PaymentClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ListResponseModel<PaymentModel>> GetPayments(int skip, int take)
        {
            ListResponseModel<PaymentModel> response = await _httpClient.GetFromJsonAsync<ListResponseModel<PaymentModel>>($"/api/payments?skip={skip}&take={take}");

            return response;
        }
    }
}
