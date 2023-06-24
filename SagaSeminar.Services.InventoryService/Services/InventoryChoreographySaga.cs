using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.InventoryService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.InventoryService.Services
{
    public class InventoryChoreographySaga : BaseSagaConsumer, IInventoryChoreographySaga
    {
        public InventoryChoreographySaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(InventoryChoreographySaga);

        public async Task HandleReverseInventoryDeliveryWhenDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(DeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                DeliveryFailedEvent @event = JsonConvert.DeserializeObject<DeliveryFailedEvent>(message.Message.Value);

                IInventoryService inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                await inventoryService.ReverseDelivery(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleInventoryDeliveryWhenOrderPaymentCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(PaymentCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                PaymentCreatedEvent @event = JsonConvert.DeserializeObject<PaymentCreatedEvent>(message.Message.Value);

                IInventoryService inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                await inventoryService.CreateInventoryDeliveryNote(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => !globalConfig.UseOrchestratorSaga;
    }
}
