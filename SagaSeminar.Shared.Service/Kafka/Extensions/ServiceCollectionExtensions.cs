using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SagaSeminar.Shared.Service.Kafka.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafka(this IServiceCollection services,
            IConfiguration configuration, bool adminEnabled = false)
        {
            IConfigurationSection consumerConfigSection = configuration.GetSection("Kafka:ConsumerConfig");

            if (adminEnabled)
            {
                services.AddSingleton(provider =>
                {
                    IConfiguration configuration = provider.GetRequiredService<IConfiguration>();

                    AdminClientConfig adminConfig = configuration.GetSection("Kafka:AdminConfig").Get<AdminClientConfig>();

                    return new AdminClientBuilder(adminConfig).Build();
                });
            }

            return services.AddSingleton(provider =>
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();

                ProducerConfig producerConfig = configuration.GetSection("Kafka:ProducerConfig").Get<ProducerConfig>();

                return new ProducerBuilder<string, string>(producerConfig).Build();
            }).Configure<ConsumerConfig>((opt) => consumerConfigSection.Bind(opt));
        }
    }
}
