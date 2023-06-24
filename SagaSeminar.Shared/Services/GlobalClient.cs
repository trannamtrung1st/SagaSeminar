using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;
using System.Net.Http.Json;

namespace SagaSeminar.Shared.Services
{
    public class GlobalClient : IGlobalClient
    {
        private readonly HttpClient _httpClient;

        public GlobalClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GlobalConfig> GetGlobalConfig()
        {
            GlobalConfig response = await _httpClient.GetFromJsonAsync<GlobalConfig>($"/api/global-config");

            return response;
        }

        public async Task<TransactionDetailsModel> GetTransactionDetails(Guid transactionId)
        {
            TransactionDetailsModel response = await _httpClient
                .GetFromJsonAsync<TransactionDetailsModel>($"/api/transactions/{transactionId}");

            return response;
        }

        public async Task<ListResponseModel<TransactionListingModel>> GetTransactions(int skip, int take)
        {
            ListResponseModel<TransactionListingModel> response = await _httpClient
                .GetFromJsonAsync<ListResponseModel<TransactionListingModel>>($"/api/transactions?skip={skip}&take={take}");

            return response;
        }

        public async Task RetryOrderTransaction(Guid transactionId, Guid sagaTransactionId)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"/api/orders/transactions/{transactionId}/retry/{sagaTransactionId}", new { });

            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateGlobalConfig(GlobalConfig config)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync("/api/global-config", config);

            response.EnsureSuccessStatusCode();
        }
    }
}
