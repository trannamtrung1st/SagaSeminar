namespace SagaSeminar.Shared.Models
{
    public class SagaTransactionUpdateModel
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public bool Retryable { get; set; }
        public string Response { get; set; }
        public string MessagePayload { get; set; }
    }
}
