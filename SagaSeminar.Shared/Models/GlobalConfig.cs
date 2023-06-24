namespace SagaSeminar.Shared.Models
{
    public class GlobalConfig
    {
        public int DelayMs { get; set; }
        public bool ShouldCreateOrderFail { get; set; }
        public bool ShouldProcessPaymentFail { get; set; }
        public bool ShouldInventoryDeliveryFail { get; set; }
        public bool ShouldDeliveryFail { get; set; }
        public int CompleteOrderRetryCount { get; set; }
        public bool UseOrchestratorSaga { get; set; }
        public bool UseReplayTechnique { get; set; }
        public bool ShouldResetData { get; set; }
    }
}
