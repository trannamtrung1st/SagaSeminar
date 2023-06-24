namespace SagaSeminar.Shared.Models
{
    public class TransactionModel
    {
        public Guid Id { get; set; }
        public DateTime StartedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string Status { get; set; }
    }
}
