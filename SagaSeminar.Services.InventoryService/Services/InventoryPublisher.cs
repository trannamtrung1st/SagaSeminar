using Confluent.Kafka;
using Newtonsoft.Json;
using SagaSeminar.Services.InventoryService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.InventoryService.Services
{
    public class InventoryPublisher : IInventoryPublisher
    {
        private readonly IProducer<string, string> _producer;

        public InventoryPublisher(IProducer<string, string> producer)
        {
            _producer = producer;
        }

        public async Task PublishInventoryDeliveryFailed(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new InventoryDeliveryFailedEvent
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(InventoryDeliveryFailedEvent),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }

        public async Task PublishInventoryDeliveryNoteCreated(InventoryNoteModel model)
        {
            string message = JsonConvert.SerializeObject(new InventoryDeliveryNoteCreatedEvent
            {
                Model = model
            });

            await _producer.ProduceAsync(nameof(InventoryDeliveryNoteCreatedEvent),
                new Message<string, string>
                {
                    Key = model.Id.ToString(),
                    Value = message
                });
        }
    }
}
