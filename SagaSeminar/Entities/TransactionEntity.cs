namespace SagaSeminar.Entities
{
    public class TransactionEntity
    {
        public Guid Id { get; set; }
        public DateTime StartedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string Status { get; set; }
        public Guid? RunningTransactionId { get; set; }

        public virtual SagaTransactionEntity RunningTransaction { get; set; }
        public virtual ICollection<SagaTransactionEntity> SagaTransactions { get; set; }
    }
}
