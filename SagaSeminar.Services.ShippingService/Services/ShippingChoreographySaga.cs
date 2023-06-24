using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.ShippingService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.ShippingService.Services
{
    public class ShippingChoreographySaga : BaseSagaConsumer, IShippingChoreographySaga
    {
        public ShippingChoreographySaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(ShippingChoreographySaga);

        public async Task HandleCreateDeliveryWhenInventoryGoodsDelivered(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(InventoryDeliveryNoteCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                InventoryDeliveryNoteCreatedEvent @event = JsonConvert.DeserializeObject<InventoryDeliveryNoteCreatedEvent>(message.Message.Value);

                IShippingService shippingService = scope.ServiceProvider.GetRequiredService<IShippingService>();

                await shippingService.CreateDelivery(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => !globalConfig.UseOrchestratorSaga;
    }
}
