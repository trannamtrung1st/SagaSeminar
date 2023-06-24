using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.PaymentService.Services.Interfaces;
using SagaSeminar.Shared.Commands;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.PaymentService.Services
{
    public class PaymentOrchestratorSaga : BaseSagaConsumer, IPaymentOrchestratorSaga
    {
        public PaymentOrchestratorSaga(IServiceProvider serviceProvider, IGlobalConfigReader globalConfigReader, IOptions<ConsumerConfig> consumerConfigOptions) : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(PaymentOrchestratorSaga);

        public async Task HandleCancelPayment(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(CancelPaymentCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                CancelPaymentCommand command = JsonConvert.DeserializeObject<CancelPaymentCommand>(message.Message.Value);

                IPaymentService paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                await paymentService.CancelPayment(command.TransactionId, command.Note);

            }, cancellationToken: cancellationToken);
        }

        public async Task HandleProcessPayment(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(ProcessPaymentCommand), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ProcessPaymentCommand command = JsonConvert.DeserializeObject<ProcessPaymentCommand>(message.Message.Value);

                IPaymentService paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                await paymentService.CreatePaymentFromOrder(command.FromOrder);

            }, cancellationToken: cancellationToken);
        }

        protected override bool Enabled(GlobalConfig globalConfig) => globalConfig.UseOrchestratorSaga;
    }
}
