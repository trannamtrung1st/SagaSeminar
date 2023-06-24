using StackExchange.Redis;

namespace SagaSeminar.Shared.Service.Utils
{
    public static class RedisHelper
    {
        public static ConnectionMultiplexer GetConnectionMultiplexer(string endpoint,
            bool allowAdmin)
        {
            ConfigurationOptions cfg = new ConfigurationOptions()
            {
                AllowAdmin = allowAdmin
            };

            cfg.EndPoints.Add(endpoint);

            string connStr = cfg.ToString();

            return ConnectionMultiplexer.Connect(connStr);
        }
    }
}
