using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.InventoryService.Services.Interfaces;
using SagaSeminar.Shared.Commands;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.InventoryService.Services
{
    public class InventoryOrchestratorSaga : BaseSagaConsumer, IInventoryOrchestratorSaga
    {
        public InventoryOrchestratorSaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(InventoryOrchestratorSaga);

        public async Task HandleInventoryDelivery(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(InventoryDeliveryCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                InventoryDeliveryCommand command = JsonConvert.DeserializeObject<InventoryDeliveryCommand>(message.Message.Value);

                IInventoryService inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                await inventoryService.CreateInventoryDeliveryNote(command.FromPayment);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleReverseInventoryDelivery(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(ReverseInventoryDeliveryCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ReverseInventoryDeliveryCommand command = JsonConvert.DeserializeObject<ReverseInventoryDeliveryCommand>(message.Message.Value);

                IInventoryService inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

                await inventoryService.ReverseDelivery(command.TransactionId, command.Note);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => globalConfig.UseOrchestratorSaga;
    }
}
