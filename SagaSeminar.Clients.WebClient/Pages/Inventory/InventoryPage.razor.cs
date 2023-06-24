using AntDesign;
using AntDesign.TableModels;
using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Inventory
{
    public partial class InventoryPage
    {
        const int PageSize = 10;

        [Inject]
        IInventoryClient InventoryClient { get; set; }

        IEnumerable<InventoryNoteModel> List { get; set; }
        int AvailableQuantity { get; set; }
        int Total { get; set; }
        int PageIndex { get; set; }
        bool Loading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Loading = true;

            PageIndex = 1;

            List<Task> tasks = new List<Task>()
            {
                FetchInventoryNotes(PageIndex),
                FetchAvailableQuantity()
            };

            await Task.WhenAll(tasks);

            Loading = false;
        }

        void OnRowClick(RowData<InventoryNoteModel> row)
        {
            Console.WriteLine($"Row {row.Data.Id} was clicked.");
        }

        async Task Refresh()
        {
            Loading = true;

            await FetchInventoryNotes(PageIndex);

            Loading = false;
        }

        async Task OnPageIndexChange(PaginationEventArgs args)
        {
            Loading = true;

            PageIndex = args.Page;

            await FetchInventoryNotes(PageIndex);

            Loading = false;
        }

        private async Task FetchInventoryNotes(int page)
        {
            ListResponseModel<InventoryNoteModel> response = await InventoryClient.GetInventoryNotes((page - 1) * PageSize, PageSize);
            List = response.List;
            Total = response.Total;
        }

        private async Task FetchAvailableQuantity()
        {
            AvailableQuantity = await InventoryClient.GetAvailableQuantity();
        }
    }
}
