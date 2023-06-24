using Newtonsoft.Json;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Services.Interfaces;
using StackExchange.Redis;

namespace SagaSeminar.Shared.Service.Services
{
    public class GlobalConfigReader : IGlobalConfigReader
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public GlobalConfigReader(ConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task Delay()
        {
            GlobalConfig config = await Read();

            if (config.DelayMs > 0)
            {
                await Task.Delay(config.DelayMs);
            }
        }

        public async Task<GlobalConfig> Read()
        {
            IDatabase db = _connectionMultiplexer.GetDatabase();

            RedisValue data = await db.StringGetAsync(nameof(GlobalConfig));

            GlobalConfig config = !string.IsNullOrEmpty(data)
                ? JsonConvert.DeserializeObject<GlobalConfig>(data)
                : new GlobalConfig();

            return config;
        }

        public async Task ThrowIfShould(Func<GlobalConfig, bool> predicate, Exception ex)
        {
            GlobalConfig config = await Read();

            if (predicate(config))
            {
                throw ex;
            }
        }
    }
}
