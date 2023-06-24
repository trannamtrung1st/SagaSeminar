using SagaSeminar.Shared.Models;

namespace SagaSeminar.Hubs.Interfaces
{
    public interface ILogHubClient
    {
        Task HandleLog(LogModel model);
    }
}
