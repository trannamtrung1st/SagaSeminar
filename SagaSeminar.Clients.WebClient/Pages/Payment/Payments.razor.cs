using AntDesign;
using AntDesign.TableModels;
using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Payment
{
    public partial class Payments
    {
        const int PageSize = 10;

        [Inject]
        IPaymentClient PaymentClient { get; set; }

        IEnumerable<PaymentModel> List { get; set; }
        int Total { get; set; }
        int PageIndex { get; set; }
        bool Loading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            PageIndex = 1;

            await FetchPayments(PageIndex);
        }

        void OnRowClick(RowData<PaymentModel> row)
        {
            Console.WriteLine($"Row {row.Data.Id} was clicked.");
        }

        async Task Refresh()
        {
            await FetchPayments(PageIndex);
        }

        async Task OnPageIndexChange(PaginationEventArgs args)
        {
            PageIndex = args.Page;

            await FetchPayments(PageIndex);
        }

        private async Task FetchPayments(int page)
        {
            Loading = true;

            ListResponseModel<PaymentModel> response = await PaymentClient.GetPayments((page - 1) * PageSize, PageSize);
            List = response.List;
            Total = response.Total;

            Loading = false;
        }
    }
}
