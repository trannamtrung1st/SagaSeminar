using AntDesign;
using AntDesign.TableModels;
using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Shipping
{
    public partial class Deliveries
    {
        const int PageSize = 10;

        [Inject]
        IShippingClient ShippingClient { get; set; }

        IEnumerable<DeliveryModel> List { get; set; }
        int PageIndex { get; set; }
        int Total { get; set; }
        bool Loading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            PageIndex = 1;

            await FetchDeliveries(PageIndex);
        }

        void OnRowClick(RowData<DeliveryModel> row)
        {
            Console.WriteLine($"Row {row.Data.Id} was clicked.");
        }

        async Task Refresh()
        {
            await FetchDeliveries(PageIndex);
        }

        async Task OnPageIndexChange(PaginationEventArgs args)
        {
            PageIndex = args.Page;

            await FetchDeliveries(PageIndex);
        }

        private async Task FetchDeliveries(int page)
        {
            Loading = true;

            ListResponseModel<DeliveryModel> response = await ShippingClient.GetDeliveries((page - 1) * PageSize, PageSize);
            List = response.List;
            Total = response.Total;

            Loading = false;
        }
    }
}
