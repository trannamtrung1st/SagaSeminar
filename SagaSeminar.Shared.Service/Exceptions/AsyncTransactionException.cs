namespace SagaSeminar.Shared.Service.Exceptions
{
    public class AsyncTransactionException : Exception
    {
        public Guid TransactionId { get; }

        public AsyncTransactionException(Guid transactionId)
        {
            TransactionId = transactionId;
        }

        public override string Message => $"Transaction {TransactionId} is performing async operations";
    }
}
