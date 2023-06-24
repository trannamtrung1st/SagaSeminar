namespace SagaSeminar.Shared.Models
{
    public class SagaTransactionModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string Response { get; set; }
        public bool Retryable { get; set; }
        public DateTime StartedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}
