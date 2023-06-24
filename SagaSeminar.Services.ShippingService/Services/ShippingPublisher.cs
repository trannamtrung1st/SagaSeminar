using Confluent.Kafka;
using Newtonsoft.Json;
using SagaSeminar.Services.ShippingService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.ShippingService.Services
{
    public class ShippingPublisher : IShippingPublisher
    {
        private readonly IProducer<string, string> _producer;

        public ShippingPublisher(IProducer<string, string> producer)
        {
            _producer = producer;
        }

        public async Task PublishDeliveryCreated(DeliveryModel model)
        {
            string message = JsonConvert.SerializeObject(new DeliveryCreatedEvent
            {
                Model = model
            });

            await _producer.ProduceAsync(nameof(DeliveryCreatedEvent),
                new Message<string, string>
                {
                    Key = model.Id.ToString(),
                    Value = message
                });
        }

        public async Task PublishDeliveryFailed(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new DeliveryFailedEvent
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(DeliveryFailedEvent),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }
    }
}
