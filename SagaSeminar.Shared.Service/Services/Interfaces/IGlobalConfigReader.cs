using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Service.Services.Interfaces
{
    public interface IGlobalConfigReader
    {
        Task<GlobalConfig> Read();
        Task ThrowIfShould(Func<GlobalConfig, bool> predicate, Exception ex);
        Task Delay();
    }
}
