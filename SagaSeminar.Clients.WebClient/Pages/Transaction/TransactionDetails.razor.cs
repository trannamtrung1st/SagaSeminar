using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Transaction
{
    public partial class TransactionDetails
    {
        [Inject]
        IGlobalClient GlobalClient { get; set; }

        [Parameter]
        public string Id { get; set; }

        Guid TransactionId { get; set; }
        TransactionDetailsModel Model { get; set; }
        bool Loading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            TransactionId = Guid.Parse(Id);

            await FetchTransactionDetails();
        }

        async Task Refresh()
        {
            await FetchTransactionDetails();
        }

        Func<Task> OnRetry(SagaTransactionModel transaction)
        {
            return async () =>
            {
                Loading = true;

                await GlobalClient.RetryOrderTransaction(TransactionId, transaction.Id);

                Loading = false;
            };
        }

        private async Task FetchTransactionDetails()
        {
            Loading = true;

            Model = await GlobalClient.GetTransactionDetails(TransactionId);

            Loading = false;
        }
    }
}
