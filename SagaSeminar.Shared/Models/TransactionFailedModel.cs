namespace SagaSeminar.Shared.Models
{
    public class TransactionFailedModel
    {
        public Guid TransactionId { get; set; }
        public string Note { get; set; }
    }
}
