using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.ShippingService.Services.Interfaces;
using SagaSeminar.Shared.Commands;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.ShippingService.Services
{
    public class ShippingOrchestratorSaga : BaseSagaConsumer, IShippingOrchestratorSaga
    {
        public ShippingOrchestratorSaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(ShippingOrchestratorSaga);

        public async Task HandleProcessDelivery(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(ProcessDeliveryCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ProcessDeliveryCommand command = JsonConvert.DeserializeObject<ProcessDeliveryCommand>(message.Message.Value);

                IShippingService shippingService = scope.ServiceProvider.GetRequiredService<IShippingService>();

                await shippingService.CreateDelivery(command.FromNote);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => globalConfig.UseOrchestratorSaga;
    }
}
