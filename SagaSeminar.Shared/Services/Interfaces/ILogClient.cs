using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Service.Interfaces
{
    public interface ILogClient
    {
        Task Log(string log);
        Task<IDisposable> HandleLog(Func<LogModel, Task> handleLog);
    }
}
