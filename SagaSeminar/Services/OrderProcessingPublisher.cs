using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SagaSeminar.Entities;
using SagaSeminar.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Commands;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using TransactionNames = SagaSeminar.Shared.Constants.TransactionNames;

namespace SagaSeminar.Services
{
    public class OrderProcessingPublisher : IOrderProcessingPublisher
    {
        private readonly IProducer<string, string> _producer;
        private readonly IRepository<SagaDbContext, TransactionEntity> _transactionRepository;

        public OrderProcessingPublisher(IProducer<string, string> producer,
            IRepository<SagaDbContext, TransactionEntity> transactionRepository)
        {
            _producer = producer;
            _transactionRepository = transactionRepository;
        }

        public async Task ProcessPayment(OrderModel order)
        {
            string message = JsonConvert.SerializeObject(new ProcessPaymentCommand
            {
                FromOrder = order,
            });

            await _producer.ProduceAsync(nameof(ProcessPaymentCommand),
                new Message<string, string>
                {
                    Key = order.Id.ToString(),
                    Value = message
                });
        }

        public async Task InventoryDelivery(PaymentModel payment)
        {
            string message = JsonConvert.SerializeObject(new InventoryDeliveryCommand
            {
                FromPayment = payment
            });

            await _producer.ProduceAsync(nameof(InventoryDeliveryCommand),
                new Message<string, string>
                {
                    Key = payment.Id.ToString(),
                    Value = message
                });
        }

        public async Task ProcessDelivery(InventoryNoteModel note)
        {
            string message = JsonConvert.SerializeObject(new ProcessDeliveryCommand
            {
                FromNote = note
            });

            await _producer.ProduceAsync(nameof(ProcessDeliveryCommand),
                new Message<string, string>
                {
                    Key = note.Id.ToString(),
                    Value = message
                });
        }

        public async Task CompleteOrder(DeliveryModel delivery)
        {
            string message = JsonConvert.SerializeObject(new CompleteOrderCommand
            {
                FromDelivery = delivery
            });

            await _producer.ProduceAsync(nameof(CompleteOrderCommand),
                new Message<string, string>
                {
                    Key = delivery.Id.ToString(),
                    Value = message
                });
        }

        public async Task ReverseInventoryDelivery(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new ReverseInventoryDeliveryCommand
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(ReverseInventoryDeliveryCommand),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }

        public async Task CancelPayment(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new CancelPaymentCommand
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(CancelPaymentCommand),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }

        public async Task CancelOrder(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new CancelOrderCommand
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(CancelOrderCommand),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }

        public async Task Retry(Guid transactionId, Guid sagaTransactionId)
        {
            TransactionEntity entity = await _transactionRepository.Query()
                .Include(e => e.SagaTransactions)
                .Where(e => e.Id == transactionId)
                .FirstOrDefaultAsync();

            SagaTransactionEntity saga = entity?.SagaTransactions?.FirstOrDefault(s => s.Id == sagaTransactionId);

            if (entity == null || saga == null) throw new Exception("Entity not found!");

            if (!saga.Retryable) throw new Exception("This saga transaction is not retryable!");

            if (saga.Status == Constants.TransactionStatus.Successful) throw new Exception("Transaction already completed successfully!");

            switch (saga.Name)
            {
                case TransactionNames.CompleteOrder:
                    {
                        DeliveryModel model = JsonConvert.DeserializeObject<DeliveryModel>(saga.MessagePayload);

                        await CompleteOrder(model);

                        break;
                    }
                default: throw new NotSupportedException();
            }
        }
    }
}
