namespace SagaSeminar.Entities
{
    public class SagaTransactionEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public bool Retryable { get; set; }
        public string Response { get; set; }
        public string MessagePayload { get; set; }
        public DateTime StartedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public Guid TransactionId { get; set; }

        public virtual TransactionEntity Transaction { get; set; }
    }
}
