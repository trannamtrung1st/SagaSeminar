using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;
using TransactionNames = SagaSeminar.Shared.Constants.TransactionNames;

namespace SagaSeminar.Services
{
    public class OrderProcessingOrchestrator : BaseSagaConsumer, IOrderProcessingOrchestrator
    {
        public OrderProcessingOrchestrator(
            IServiceProvider serviceProvider,
            IGlobalConfigReader globalConfigReader,
            IOptions<ConsumerConfig> consumerConfigOptions)
            : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
        }

        protected override string GroupId => nameof(OrderProcessingOrchestrator);

        public async Task Start(CancellationToken cancellationToken)
        {
            await HandleOrderCreated(cancellationToken);

            await HandlePaymentCreated(cancellationToken);

            await HandleInventoryDeliveryNoteCreated(cancellationToken);

            await HandleDeliveryCreated(cancellationToken);

            await HandleOrderCompleted(cancellationToken);

            await HandleDeliveryFailed(cancellationToken);

            await HandleInventoryDeliveryFailed(cancellationToken);

            await HandlePaymentFailed(cancellationToken);

            await HandleCompleteOrderFailed(cancellationToken);
        }

        #region Handlers

        protected async Task HandleDeliveryCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(DeliveryCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                DeliveryCreatedEvent @event = JsonConvert.DeserializeObject<DeliveryCreatedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.Model.TransactionId, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel { Name = TransactionNames.ProcessDelivery, Status = Constants.TransactionStatus.Successful },
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.CompleteOrder,
                        Status = Constants.TransactionStatus.Triggered,
                        Retryable = true,
                        MessagePayload = JsonConvert.SerializeObject(@event.Model)
                    });

                await publisher.CompleteOrder(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandleDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(DeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                DeliveryFailedEvent @event = JsonConvert.DeserializeObject<DeliveryFailedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.TransactionId, status: Constants.TransactionStatus.Failed,
                    new SagaTransactionUpdateModel { Name = TransactionNames.ProcessDelivery, Status = Constants.TransactionStatus.Failed, Note = @event.Note },
                    new SagaTransactionUpdateModel { Name = TransactionNames.CancelOrder, Status = Constants.TransactionStatus.Triggered, Note = @event.Note },
                    new SagaTransactionUpdateModel { Name = TransactionNames.CancelPayment, Status = Constants.TransactionStatus.Triggered, Note = @event.Note },
                    new SagaTransactionUpdateModel { Name = TransactionNames.ReverseInventoryDelivery, Status = Constants.TransactionStatus.Triggered, Note = @event.Note });

                await Task.WhenAll(
                    publisher.CancelOrder(@event.TransactionId, @event.Note),
                    publisher.CancelPayment(@event.TransactionId, @event.Note),
                    publisher.ReverseInventoryDelivery(@event.TransactionId, @event.Note));

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandleInventoryDeliveryFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(InventoryDeliveryFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                InventoryDeliveryFailedEvent @event = JsonConvert.DeserializeObject<InventoryDeliveryFailedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.TransactionId, status: Constants.TransactionStatus.Failed,
                    new SagaTransactionUpdateModel { Name = TransactionNames.InventoryDelivery, Status = Constants.TransactionStatus.Failed, Note = @event.Note },
                    new SagaTransactionUpdateModel { Name = TransactionNames.CancelOrder, Status = Constants.TransactionStatus.Triggered, Note = @event.Note },
                    new SagaTransactionUpdateModel { Name = TransactionNames.CancelPayment, Status = Constants.TransactionStatus.Triggered, Note = @event.Note });

                await Task.WhenAll(
                    publisher.CancelOrder(@event.TransactionId, @event.Note),
                    publisher.CancelPayment(@event.TransactionId, @event.Note));

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandleInventoryDeliveryNoteCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(InventoryDeliveryNoteCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                InventoryDeliveryNoteCreatedEvent @event = JsonConvert.DeserializeObject<InventoryDeliveryNoteCreatedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.Model.TransactionId, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel { Name = TransactionNames.InventoryDelivery, Status = Constants.TransactionStatus.Successful },
                    new SagaTransactionUpdateModel { Name = TransactionNames.ProcessDelivery, Status = Constants.TransactionStatus.Triggered });

                await publisher.ProcessDelivery(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandleOrderCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(OrderCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                OrderCreatedEvent @event = JsonConvert.DeserializeObject<OrderCreatedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.Model.TransactionId, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel { Name = TransactionNames.CreateOrder, Status = Constants.TransactionStatus.Successful },
                    new SagaTransactionUpdateModel { Name = TransactionNames.ProcessPayment, Status = Constants.TransactionStatus.Triggered });

                await publisher.ProcessPayment(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandleOrderCompleted(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(OrderCompletedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

                OrderCompletedEvent @event = JsonConvert.DeserializeObject<OrderCompletedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.TransactionId, status: Constants.TransactionStatus.Successful,
                    new SagaTransactionUpdateModel { Name = TransactionNames.CompleteOrder, Status = Constants.TransactionStatus.Successful });

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandleCompleteOrderFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(CompleteOrderFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

                CompleteOrderFailedEvent @event = JsonConvert.DeserializeObject<CompleteOrderFailedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.TransactionId, status: Constants.TransactionStatus.Failed,
                    new SagaTransactionUpdateModel { Name = TransactionNames.CompleteOrder, Status = Constants.TransactionStatus.Failed, Note = @event.Note });

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandlePaymentCreated(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(PaymentCreatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                PaymentCreatedEvent @event = JsonConvert.DeserializeObject<PaymentCreatedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.Model.TransactionId, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel { Name = TransactionNames.ProcessPayment, Status = Constants.TransactionStatus.Successful },
                    new SagaTransactionUpdateModel { Name = TransactionNames.InventoryDelivery, Status = Constants.TransactionStatus.Triggered });

                await publisher.InventoryDelivery(@event.Model);

            }, cancellationToken: cancellationToken);
        }

        protected async Task HandlePaymentFailed(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(PaymentFailedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();

                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                PaymentFailedEvent @event = JsonConvert.DeserializeObject<PaymentFailedEvent>(message.Message.Value);

                await transactionService.UpdateTransaction(
                    id: @event.TransactionId, status: Constants.TransactionStatus.Failed,
                    new SagaTransactionUpdateModel { Name = TransactionNames.ProcessPayment, Status = Constants.TransactionStatus.Failed, Note = @event.Note },
                    new SagaTransactionUpdateModel { Name = TransactionNames.CancelOrder, Status = Constants.TransactionStatus.Triggered, Note = @event.Note });

                await publisher.CancelOrder(@event.TransactionId, @event.Note);

            }, cancellationToken: cancellationToken);
        }

        #endregion

        protected override bool Enabled(GlobalConfig globalConfig)
            => globalConfig.UseOrchestratorSaga && !globalConfig.UseReplayTechnique;
    }
}
