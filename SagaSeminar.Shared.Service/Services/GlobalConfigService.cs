using Newtonsoft.Json;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Services.Interfaces;
using StackExchange.Redis;

namespace SagaSeminar.Shared.Service.Services
{
    public class GlobalConfigService : IGlobalConfigService
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public GlobalConfigService(ConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task Update(GlobalConfig config)
        {
            string data = JsonConvert.SerializeObject(config);

            IDatabase db = _connectionMultiplexer.GetDatabase();

            await db.StringSetAsync(nameof(GlobalConfig), data);
        }
    }
}
