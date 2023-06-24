﻿namespace SagaSeminar.Shared.Models
{
    public class PaymentModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public double Amount { get; set; }
        public string Customer { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Status { get; set; }
        public Guid TransactionId { get; set; }
        public string Note { get; set; }
    }
}
