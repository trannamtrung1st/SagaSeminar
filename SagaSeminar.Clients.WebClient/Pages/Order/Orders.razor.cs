using AntDesign;
using AntDesign.TableModels;
using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Order
{
    public partial class Orders
    {
        const int PageSize = 10;

        [Inject]
        IOrderClient OrderClient { get; set; }

        IEnumerable<OrderModel> List { get; set; }
        int Total { get; set; }
        bool Loading { get; set; }
        int PageIndex { get; set; }

        protected override async Task OnInitializedAsync()
        {
            PageIndex = 1;

            await FetchOrders(PageIndex);
        }

        async Task Refresh()
        {
            await FetchOrders(PageIndex);
        }

        void OnRowClick(RowData<OrderModel> row)
        {
            Console.WriteLine($"Row {row.Data.Id} was clicked.");
        }

        async Task OnPageIndexChange(PaginationEventArgs args)
        {
            PageIndex = args.Page;

            await FetchOrders(PageIndex);
        }

        private async Task FetchOrders(int page)
        {
            Loading = true;

            ListResponseModel<OrderModel> response = await OrderClient.GetOrders((page - 1) * PageSize, PageSize);
            List = response.List;
            Total = response.Total;

            Loading = false;
        }
    }
}
