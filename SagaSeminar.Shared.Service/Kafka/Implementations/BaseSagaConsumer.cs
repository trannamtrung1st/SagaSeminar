using Confluent.Kafka;
using Microsoft.Extensions.Options;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Shared.Service.Kafka.Implementations
{
    public abstract class BaseSagaConsumer : BaseConsumer
    {
        protected readonly IGlobalConfigReader globalConfigReader;

        protected BaseSagaConsumer(
            IServiceProvider serviceProvider,
            IGlobalConfigReader globalConfigReader,
            IOptions<ConsumerConfig> consumerConfigOptions)
            : base(serviceProvider, consumerConfigOptions)
        {
            this.globalConfigReader = globalConfigReader;
        }

        protected abstract bool Enabled(GlobalConfig globalConfig);

        protected override Task StartConsumerThread(string topic, Func<ConsumeResult<string, string>, Task> HandleMessage,
            string instanceId = null, CancellationToken cancellationToken = default)
        {
            return base.StartConsumerThread(topic, async (message) =>
            {
                GlobalConfig globalConfig = await globalConfigReader.Read();

                if (Enabled(globalConfig))
                {
                    await HandleMessage(message);
                }
            }, instanceId, cancellationToken);
        }
    }
}
