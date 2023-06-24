using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.OrderService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.OrderService.Services
{
    public class OrderChoreographySaga : BaseSagaConsumer, IOrderChoreographySaga
    {
        public OrderChoreographySaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(OrderChoreographySaga);

        public async Task HandleCompleteOrderWhenDeliveryCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(DeliveryCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                DeliveryCreatedEvent @event = JsonConvert.DeserializeObject<DeliveryCreatedEvent>(message.Message.Value);

                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.CompleteOrder(@event.Model.TransactionId);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleCancelOrderWhenDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(DeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                DeliveryFailedEvent @event = JsonConvert.DeserializeObject<DeliveryFailedEvent>(message.Message.Value);

                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.CancelOrder(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleCancelOrderWhenInventoryDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(InventoryDeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                InventoryDeliveryFailedEvent @event = JsonConvert.DeserializeObject<InventoryDeliveryFailedEvent>(message.Message.Value);

                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.CancelOrder(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleCancelOrderWhenPaymentFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(PaymentFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                PaymentFailedEvent @event = JsonConvert.DeserializeObject<PaymentFailedEvent>(message.Message.Value);

                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.CancelOrder(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => !globalConfig.UseOrchestratorSaga;
    }
}
