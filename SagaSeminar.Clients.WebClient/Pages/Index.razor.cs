using Microsoft.AspNetCore.Components;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Interfaces;

namespace SagaSeminar.Clients.WebClient.Pages
{
    public partial class Index : IDisposable
    {
        private IDisposable _logHandler;

        [Inject]
        ILogClient LogClient { get; set; }

        List<LogModel> Logs { get; set; }

        public Index()
        {
            Logs = new List<LogModel>()
            {
                new LogModel
                {
                    Color = "black",
                    Data = "Start viewing logs",
                    Source = "System",
                    Time = DateTime.Now
                }
            };
        }

        protected override async Task OnInitializedAsync()
        {
            _logHandler = await LogClient.HandleLog(HandleLog);
        }

        Task HandleLog(LogModel model)
        {
            Logs.Insert(0, model);

            StateHasChanged();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logHandler?.Dispose();
        }
    }
}
