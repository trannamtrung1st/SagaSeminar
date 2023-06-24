using AntDesign;
using AntDesign.TableModels;
using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Transaction
{
    public partial class Transactions
    {
        const int PageSize = 10;

        [Inject]
        IGlobalClient GlobalClient { get; set; }

        IEnumerable<TransactionListingModel> List { get; set; }
        int PageIndex { get; set; }
        int Total { get; set; }
        bool Loading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            PageIndex = 1;

            await FetchTransactions(PageIndex);
        }

        void OnRowClick(RowData<TransactionListingModel> row)
        {
            Console.WriteLine($"Row {row.Data.Id} was clicked.");
        }

        async Task Refresh()
        {
            await FetchTransactions(PageIndex);
        }

        async Task OnPageIndexChange(PaginationEventArgs args)
        {
            PageIndex = args.Page;

            await FetchTransactions(PageIndex);
        }

        private async Task FetchTransactions(int page)
        {
            Loading = true;

            ListResponseModel<TransactionListingModel> response = await GlobalClient.GetTransactions((page - 1) * PageSize, PageSize);
            List = response.List;
            Total = response.Total;

            Loading = false;
        }
    }
}
