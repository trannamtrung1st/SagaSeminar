namespace SagaSeminar.Shared.Service.Exceptions
{
    public class TransactionFailedException : Exception
    {
        public Guid TransactionId { get; }

        public TransactionFailedException(Guid transactionId)
        {
            TransactionId = transactionId;
        }

        public override string Message => $"Transaction {TransactionId} has failed";
    }
}
