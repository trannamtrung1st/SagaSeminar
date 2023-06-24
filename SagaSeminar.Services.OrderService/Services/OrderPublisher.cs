using Confluent.Kafka;
using Newtonsoft.Json;
using SagaSeminar.Services.OrderService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.OrderService.Services
{
    public class OrderPublisher : IOrderPublisher
    {
        private readonly IProducer<string, string> _producer;

        public OrderPublisher(IProducer<string, string> producer)
        {
            _producer = producer;
        }

        public async Task PublishCompleteOrderFailed(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new CompleteOrderFailedEvent
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(CompleteOrderFailedEvent),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }

        public async Task PublishOrderCompleted(Guid transactionId)
        {
            string message = JsonConvert.SerializeObject(new OrderCompletedEvent
            {
                TransactionId = transactionId,
            });

            await _producer.ProduceAsync(nameof(OrderCompletedEvent),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }

        public async Task PublishOrderCreated(OrderModel model)
        {
            string message = JsonConvert.SerializeObject(new OrderCreatedEvent
            {
                Model = model
            });

            await _producer.ProduceAsync(nameof(OrderCreatedEvent),
                new Message<string, string>
                {
                    Key = model.Id.ToString(),
                    Value = message
                });
        }
    }
}
