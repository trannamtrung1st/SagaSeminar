using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages.Global
{
    public partial class GlobalConfigPage
    {
        [Inject]
        IGlobalClient GlobalClient { get; set; }

        [Inject]
        MessageService Message { get; set; }

        GlobalConfig Model { get; set; }
        bool Loading { get; set; }

        public GlobalConfigPage()
        {
            Model = new GlobalConfig();
        }

        protected override async Task OnInitializedAsync()
        {
            Loading = true;

            Model = await GlobalClient.GetGlobalConfig();

            Loading = false;
        }

        async Task OnFinish(EditContext context)
        {
            try
            {
                Loading = true;

                GlobalConfig model = context.Model as GlobalConfig;

                await GlobalClient.UpdateGlobalConfig(model);

                _ = Message.Success("Successfully updated config!");

                Loading = false;
            }
            catch (Exception ex)
            {
                Loading = false;

                Console.Error.WriteLine(ex);

                _ = Message.Error("Failed to update config!");
            }
        }

        void OnFinishFailed(EditContext context)
        {
            Console.WriteLine("Invalid data");
        }
    }
}
