using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Service.Services.Interfaces
{
    public interface IGlobalConfigService
    {
        Task Update(GlobalConfig config);
    }
}
