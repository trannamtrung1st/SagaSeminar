namespace SagaSeminar.Shared.Service
{
    public static class Constants
    {
        public const int DefaultConsumerRetryAfter = 7000;

        public static class ConfigurationSections
        {
            public const string RedisEndpoint = "Redis:Endpoint";
        }
    }
}
