using Confluent.Kafka;
using Newtonsoft.Json;
using SagaSeminar.Services.PaymentService.Services.Interfaces;
using SagaSeminar.Shared.Events;
using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.PaymentService.Services
{
    public class PaymentPublisher : IPaymentPublisher
    {
        private readonly IProducer<string, string> _producer;

        public PaymentPublisher(IProducer<string, string> producer)
        {
            _producer = producer;
        }

        public async Task PublishPaymentCreated(PaymentModel model)
        {
            string message = JsonConvert.SerializeObject(new PaymentCreatedEvent
            {
                Model = model
            });

            await _producer.ProduceAsync(nameof(PaymentCreatedEvent),
                new Message<string, string>
                {
                    Key = model.Id.ToString(),
                    Value = message
                });
        }

        public async Task PublishPaymentFailed(Guid transactionId, string note)
        {
            string message = JsonConvert.SerializeObject(new PaymentFailedEvent
            {
                TransactionId = transactionId,
                Note = note
            });

            await _producer.ProduceAsync(nameof(PaymentFailedEvent),
                new Message<string, string>
                {
                    Key = transactionId.ToString(),
                    Value = message
                });
        }
    }
}
