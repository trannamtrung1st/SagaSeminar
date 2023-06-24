using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Order
{
    public partial class CreateOrder
    {
        [Inject]
        IOrderClient OrderClient { get; set; }

        [Inject]
        NavigationManager Navigation { get; set; }

        [Inject]
        MessageService Message { get; set; }

        CreateOrderModel Model { get; set; }
        bool Loading { get; set; }

        public CreateOrder()
        {
            Model = new CreateOrderModel();
        }

        async Task OnFinish(EditContext context)
        {
            try
            {
                Loading = true;

                CreateOrderModel model = context.Model as CreateOrderModel;

                await OrderClient.CreateOrder(model);

                _ = Message.Success("Successfully created order!");

                Loading = false;

                Navigation.NavigateTo("/orders");
            }
            catch (Exception ex)
            {
                Loading = false;

                Console.Error.WriteLine(ex);

                _ = Message.Error("Failed to create order!");
            }
        }

        void OnFinishFailed(EditContext context)
        {
            Console.WriteLine("Invalid data");
        }
    }
}
