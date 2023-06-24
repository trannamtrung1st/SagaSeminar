using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.OrderService.Services.Interfaces;
using SagaSeminar.Shared.Commands;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.OrderService.Services
{
    public class OrderOrchestratorSaga : BaseSagaConsumer, IOrderOrchestratorSaga
    {
        public OrderOrchestratorSaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(OrderOrchestratorSaga);

        public async Task HandleCancelOrder(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(CancelOrderCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                CancelOrderCommand command = JsonConvert.DeserializeObject<CancelOrderCommand>(message.Message.Value);

                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.CancelOrder(command.TransactionId, command.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleCompleteOrder(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(CompleteOrderCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                CompleteOrderCommand command = JsonConvert.DeserializeObject<CompleteOrderCommand>(message.Message.Value);

                IOrderService orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                await orderService.CompleteOrder(command.FromDelivery.TransactionId);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => globalConfig.UseOrchestratorSaga;
    }
}
