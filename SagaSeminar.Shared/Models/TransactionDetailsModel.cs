namespace SagaSeminar.Shared.Models
{
    public class TransactionDetailsModel : TransactionModel
    {
        public IEnumerable<SagaTransactionModel> Transactions { get; set; }
    }
}
