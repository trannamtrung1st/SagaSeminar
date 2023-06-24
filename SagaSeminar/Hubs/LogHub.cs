using Microsoft.AspNetCore.SignalR;
using SagaSeminar.Hubs.Interfaces;
using SagaSeminar.Shared.Models;

namespace SagaSeminar.Hubs
{
    public class LogHub : Hub<ILogHubClient>
    {
        public async Task Log(LogModel model)
        {
            await Clients.All.HandleLog(model);
        }
    }
}
