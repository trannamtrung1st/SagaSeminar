using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace SagaSeminar.Shared.Service.Kafka.Implementations
{
    public abstract class BaseConsumer
    {
        protected readonly IServiceProvider serviceProvider;
        protected readonly IOptions<ConsumerConfig> consumerConfigOptions;
        public BaseConsumer(IServiceProvider serviceProvider,
            IOptions<ConsumerConfig> consumerConfigOptions)
        {
            this.serviceProvider = serviceProvider;
            this.consumerConfigOptions = consumerConfigOptions;
        }

        protected abstract string GroupId { get; }

        protected virtual Task StartConsumerThread(
            string topic, Func<ConsumeResult<string, string>, Task> HandleMessage,
            string instanceId = null, CancellationToken cancellationToken = default)
        {
            Thread thread = new Thread(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        ConsumerConfig commonConfig = consumerConfigOptions.Value;
                        Dictionary<string, string> clonedConfig = new Dictionary<string, string>(commonConfig.AsEnumerable());
                        ConsumerConfig consumerConfig = new ConsumerConfig(clonedConfig)
                        {
                            GroupId = GroupId,
                            GroupInstanceId = instanceId ?? topic
                        };
                        using IConsumer<string, string> consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
                        consumer.Subscribe(topic);

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            ConsumeResult<string, string> message = consumer.Consume(cancellationToken);

                            try
                            {
                                await HandleMessage(message);
                            }
                            catch { }

                            consumer.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);

                        await Task.Delay(Constants.DefaultConsumerRetryAfter, cancellationToken);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
            return Task.CompletedTask;
        }
    }
}
