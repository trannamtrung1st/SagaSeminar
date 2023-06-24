using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SagaSeminar.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Exceptions;
using SagaSeminar.Shared.Service.Kafka.Implementations;
using SagaSeminar.Shared.Service.Services.Interfaces;
using TransactionNames = SagaSeminar.Shared.Constants.TransactionNames;

namespace SagaSeminar.Services
{
    public class OrderProcessingReplayOrchestrator : BaseSagaConsumer, IOrderProcessingOrchestrator
    {
        private readonly IProducer<string, string> _producer;

        public OrderProcessingReplayOrchestrator(
            IServiceProvider serviceProvider,
            IGlobalConfigReader globalConfigReader,
            IProducer<string, string> producer,
            IOptions<ConsumerConfig> consumerConfigOptions)
            : base(serviceProvider, globalConfigReader, consumerConfigOptions)
        {
            _producer = producer;
        }

        protected override string GroupId => nameof(OrderProcessingReplayOrchestrator);

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

            // [NOTE] Final saga handler
            await HandleSaga(cancellationToken);
        }

        protected async Task HandleSaga(CancellationToken cancellationToken)
        {
            await StartConsumerThread(nameof(TransactionUpdatedEvent), async (message) =>
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                ITransactionService transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                IOrderProcessingPublisher publisher = scope.ServiceProvider.GetRequiredService<IOrderProcessingPublisher>();

                TransactionUpdatedEvent @event = JsonConvert.DeserializeObject<TransactionUpdatedEvent>(message.Message.Value);
                Guid transactionId = @event.TransactionId;
                TransactionDetailsModel transaction = await transactionService.GetTransactionDetails(transactionId);

                try
                {
                    ThrowIfFailed(transaction);

                    OrderModel order = await GetCreatedOrder(transaction);

                    // 1st play
                    PaymentModel payment = await ProcessPayment(order, transaction, transactionService, publisher);

                    // 2nd play
                    InventoryNoteModel inventoryNote = await InventoryDelivery(payment, transaction, transactionService, publisher);

                    // 3rd play
                    DeliveryModel delivery = await ProcessDelivery(inventoryNote, transaction, transactionService, publisher);

                    // 4th play
                    await CompleteOrder(delivery, transaction, transactionService, publisher);
                }
                catch (TransactionFailedException ex)
                {
                    // x? play (with failure)
                    await Rollback(ex, transaction, transactionService, publisher);
                }
                catch (AsyncTransactionException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }, cancellationToken: cancellationToken);
        }

        private Task<OrderModel> GetCreatedOrder(TransactionDetailsModel transaction)
        {
            SagaTransactionModel localTransaction = transaction.Transactions.FirstOrDefault(t => t.Name == TransactionNames.CreateOrder);

            if (localTransaction == null) throw new Exception("Invalid transaction!");

            OrderCreatedEvent response = JsonConvert.DeserializeObject<OrderCreatedEvent>(localTransaction.Response);

            return Task.FromResult(response.Model);
        }

        private async Task<PaymentModel> ProcessPayment(
            OrderModel order, TransactionDetailsModel transaction,
            ITransactionService transactionService, IOrderProcessingPublisher publisher)
        {
            SagaTransactionModel localTransaction = transaction.Transactions.FirstOrDefault(t => t.Name == TransactionNames.ProcessPayment);

            if (localTransaction?.Status == Constants.TransactionStatus.Successful)
            {
                PaymentCreatedEvent response = JsonConvert.DeserializeObject<PaymentCreatedEvent>(localTransaction.Response);

                return response.Model;
            }

            if (localTransaction == null)
            {
                await transactionService.UpdateTransaction(
                    id: transaction.Id, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.ProcessPayment,
                        Status = Constants.TransactionStatus.Triggered
                    });

                await publisher.ProcessPayment(order);
            }

            throw new AsyncTransactionException(transaction.Id);
        }

        private async Task<InventoryNoteModel> InventoryDelivery(
            PaymentModel payment, TransactionDetailsModel transaction,
            ITransactionService transactionService, IOrderProcessingPublisher publisher)
        {
            SagaTransactionModel localTransaction = transaction.Transactions.FirstOrDefault(t => t.Name == TransactionNames.InventoryDelivery);

            if (localTransaction?.Status == Constants.TransactionStatus.Successful)
            {
                InventoryDeliveryNoteCreatedEvent response = JsonConvert.DeserializeObject<InventoryDeliveryNoteCreatedEvent>(localTransaction.Response);

                return response.Model;
            }

            if (localTransaction == null)
            {
                await transactionService.UpdateTransaction(
                    id: transaction.Id, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.InventoryDelivery,
                        Status = Constants.TransactionStatus.Triggered
                    });

                await publisher.InventoryDelivery(payment);

                throw new AsyncTransactionException(transaction.Id);
            }

            throw new AsyncTransactionException(transaction.Id);
        }

        private async Task<DeliveryModel> ProcessDelivery(
            InventoryNoteModel inventoryNote, TransactionDetailsModel transaction,
            ITransactionService transactionService, IOrderProcessingPublisher publisher)
        {
            SagaTransactionModel localTransaction = transaction.Transactions.FirstOrDefault(t => t.Name == TransactionNames.ProcessDelivery);

            if (localTransaction?.Status == Constants.TransactionStatus.Successful)
            {
                DeliveryCreatedEvent response = JsonConvert.DeserializeObject<DeliveryCreatedEvent>(localTransaction.Response);

                return response.Model;
            }

            if (localTransaction == null)
            {
                await transactionService.UpdateTransaction(
                    id: transaction.Id, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.ProcessDelivery,
                        Status = Constants.TransactionStatus.Triggered
                    });

                await publisher.ProcessDelivery(inventoryNote);
            }

            throw new AsyncTransactionException(transaction.Id);
        }

        private async Task<Guid> CompleteOrder(
            DeliveryModel delivery, TransactionDetailsModel transaction,
            ITransactionService transactionService, IOrderProcessingPublisher publisher)
        {
            SagaTransactionModel localTransaction = transaction.Transactions.FirstOrDefault(t => t.Name == TransactionNames.CompleteOrder);

            if (localTransaction?.Status == Constants.TransactionStatus.Successful)
            {
                OrderCompletedEvent response = JsonConvert.DeserializeObject<OrderCompletedEvent>(localTransaction.Response);

                return response.TransactionId;
            }

            if (localTransaction == null)
            {
                await transactionService.UpdateTransaction(
                    id: transaction.Id, status: Constants.TransactionStatus.Processing,
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.CompleteOrder,
                        Status = Constants.TransactionStatus.Triggered,
                        Retryable = true,
                        MessagePayload = JsonConvert.SerializeObject(delivery)
                    });

                await publisher.CompleteOrder(delivery);
            }

            throw new AsyncTransactionException(transaction.Id);
        }

        private void CancelOrderTask(Guid transactionId, string note,
            out Func<IOrderProcessingPublisher, Task> CancelOrder,
            out SagaTransactionUpdateModel updateModel)
        {
            CancelOrder = (publisher) => publisher.CancelOrder(transactionId, note);
            updateModel = new SagaTransactionUpdateModel
            {
                Name = TransactionNames.CancelOrder,
                Status = Constants.TransactionStatus.Triggered,
                Note = note
            };
        }

        private void CancelPaymentTask(Guid transactionId, string note,
            out Func<IOrderProcessingPublisher, Task> CancelPayment,
            out SagaTransactionUpdateModel updateModel)
        {
            CancelPayment = (publisher) => publisher.CancelPayment(transactionId, note);
            updateModel = new SagaTransactionUpdateModel
            {
                Name = TransactionNames.CancelPayment,
                Status = Constants.TransactionStatus.Triggered,
                Note = note
            };
        }

        private void ReverseInventoryDeliveryTask(Guid transactionId, string note,
            out Func<IOrderProcessingPublisher, Task> ReverseInventoryDelivery,
            out SagaTransactionUpdateModel updateModel)
        {
            ReverseInventoryDelivery = (publisher) => publisher.ReverseInventoryDelivery(transactionId, note);
            updateModel = new SagaTransactionUpdateModel
            {
                Name = TransactionNames.ReverseInventoryDelivery,
                Status = Constants.TransactionStatus.Triggered,
                Note = note
            };
        }

        private void ThrowIfFailed(TransactionDetailsModel transaction)
        {
            if (transaction.Status == Constants.TransactionStatus.Failed)
            {
                throw new TransactionFailedException(transaction.Id);
            }
        }

        private async Task Rollback(TransactionFailedException exception, TransactionDetailsModel transaction,
            ITransactionService transactionService, IOrderProcessingPublisher publisher)
        {
            SagaTransactionModel latestFailure = transaction.Transactions
                .OrderByDescending(t => t.LastUpdatedTime)
                .Where(t => t.Status == Constants.TransactionStatus.Failed)
                .FirstOrDefault();

            bool reverseInventoryDelivery = false;
            bool cancelPayment = false;
            bool cancelOrder = false;

            switch (latestFailure.Name)
            {
                case TransactionNames.ProcessDelivery:
                    {
                        reverseInventoryDelivery = true;
                        cancelPayment = true;
                        cancelOrder = true;
                        break;
                    }
                case TransactionNames.InventoryDelivery:
                    {
                        cancelPayment = true;
                        cancelOrder = true;
                        break;
                    }
                case TransactionNames.ProcessPayment:
                    {
                        cancelOrder = true;
                        break;
                    }
            }

            List<Func<IOrderProcessingPublisher, Task>> tasks = new List<Func<IOrderProcessingPublisher, Task>>();
            List<SagaTransactionUpdateModel> sagaTransactionUpdates = new List<SagaTransactionUpdateModel>();

            if (cancelOrder)
            {
                CancelOrderTask(transaction.Id, latestFailure.Note,
                    out Func<IOrderProcessingPublisher, Task> CancelOrder,
                    out SagaTransactionUpdateModel updateModel);

                tasks.Add(CancelOrder);
                sagaTransactionUpdates.Add(updateModel);
            }

            if (cancelPayment)
            {
                CancelPaymentTask(transaction.Id, latestFailure.Note,
                    out Func<IOrderProcessingPublisher, Task> CancelPayment,
                    out SagaTransactionUpdateModel updateModel);

                tasks.Add(CancelPayment);
                sagaTransactionUpdates.Add(updateModel);
            }

            if (reverseInventoryDelivery)
            {
                ReverseInventoryDeliveryTask(transaction.Id, latestFailure.Note,
                    out Func<IOrderProcessingPublisher, Task> ReverseInventoryDelivery,
                    out SagaTransactionUpdateModel updateModel);

                tasks.Add(ReverseInventoryDelivery);
                sagaTransactionUpdates.Add(updateModel);
            }

            await transactionService.UpdateTransaction(
                id: transaction.Id, status: Constants.TransactionStatus.Failed,
                sagaTransactionUpdates.ToArray());

            await Task.WhenAll(tasks.Select(PerformTask => PerformTask(publisher)));
        }

        private async Task NotifyTransactionUpdated(Guid transactionId)
        {
            string message = JsonConvert.SerializeObject(new TransactionUpdatedEvent
            {
                TransactionId = transactionId,
            });

            await _producer.ProduceAsync(nameof(TransactionUpdatedEvent),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.ProcessDelivery,
                        Status = Constants.TransactionStatus.Successful,
                        Response = message.Message.Value
                    });

                await NotifyTransactionUpdated(@event.Model.TransactionId);

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.ProcessDelivery,
                        Status = Constants.TransactionStatus.Failed,
                        Note = @event.Note
                    });

                await NotifyTransactionUpdated(@event.TransactionId);

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.InventoryDelivery,
                        Status = Constants.TransactionStatus.Failed,
                        Note = @event.Note
                    });

                await NotifyTransactionUpdated(@event.TransactionId);

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.InventoryDelivery,
                        Status = Constants.TransactionStatus.Successful,
                        Response = message.Message.Value
                    });

                await NotifyTransactionUpdated(@event.Model.TransactionId);

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.CreateOrder,
                        Status = Constants.TransactionStatus.Successful,
                        Response = message.Message.Value
                    });

                await NotifyTransactionUpdated(@event.Model.TransactionId);

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.CompleteOrder,
                        Status = Constants.TransactionStatus.Successful,
                        Response = message.Message.Value
                    });

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.CompleteOrder,
                        Status = Constants.TransactionStatus.Failed,
                        Note = @event.Note
                    });

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.ProcessPayment,
                        Status = Constants.TransactionStatus.Successful,
                        Response = message.Message.Value
                    });

                await NotifyTransactionUpdated(@event.Model.TransactionId);

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
                    new SagaTransactionUpdateModel
                    {
                        Name = TransactionNames.ProcessPayment,
                        Status = Constants.TransactionStatus.Failed,
                        Note = @event.Note
                    });

                await NotifyTransactionUpdated(@event.TransactionId);

            }, cancellationToken: cancellationToken);
        }

        #endregion

        protected override bool Enabled(GlobalConfig globalConfig)
            => globalConfig.UseOrchestratorSaga && globalConfig.UseReplayTechnique;
    }
}
