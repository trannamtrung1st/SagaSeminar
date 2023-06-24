namespace SagaSeminar.Shared
{
    public static class Constants
    {
        public const int DeleteTopicDelay = 5000;
        public const int SagaStartDelay = 10000;

        public static class ConfigurationSections
        {
            public const string ApiInfo = nameof(ApiInfo);
        }

        public static class OrderStatus
        {
            public const string Completed = nameof(Completed);
            public const string Processing = nameof(Processing);
            public const string Cancelled = nameof(Cancelled);
        }

        public static class TransactionStatus
        {
            public const string Processing = nameof(Processing);
            public const string Successful = nameof(Successful);
            public const string Failed = nameof(Failed);
            public const string Triggered = nameof(Triggered);
        }

        public static class PaymentStatus
        {
            public const string Successful = nameof(Successful);
            public const string Cancelled = nameof(Cancelled);
        }

        public static class TransactionNames
        {
            public const string CreateOrder = nameof(CreateOrder);
            public const string ProcessPayment = nameof(ProcessPayment);
            public const string InventoryDelivery = nameof(InventoryDelivery);
            public const string ProcessDelivery = nameof(ProcessDelivery);
            public const string CompleteOrder = nameof(CompleteOrder);
            public const string CancelOrder = nameof(CancelOrder);
            public const string CancelPayment = nameof(CancelPayment);
            public const string ReverseInventoryDelivery = nameof(ReverseInventoryDelivery);
        }
    }
}
