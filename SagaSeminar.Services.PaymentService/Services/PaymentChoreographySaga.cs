using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.PaymentService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.PaymentService.Services
{
    public class PaymentChoreographySaga : BaseSagaConsumer, IPaymentChoreographySaga
    {
        public PaymentChoreographySaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(PaymentChoreographySaga);

        public async Task HandleCancelPaymentWhenDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(DeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                DeliveryFailedEvent @event = JsonConvert.DeserializeObject<DeliveryFailedEvent>(message.Message.Value);

                IPaymentService paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                await paymentService.CancelPayment(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleCancelPaymentWhenInventoryDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(InventoryDeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                InventoryDeliveryFailedEvent @event = JsonConvert.DeserializeObject<InventoryDeliveryFailedEvent>(message.Message.Value);

                IPaymentService paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                await paymentService.CancelPayment(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleCreatePaymentWhenOrderCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(OrderCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                OrderCreatedEvent @event = JsonConvert.DeserializeObject<OrderCreatedEvent>(message.Message.Value);

                IPaymentService paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                await paymentService.CreatePaymentFromOrder(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => !globalConfig.UseOrchestratorSaga;
    }
}
