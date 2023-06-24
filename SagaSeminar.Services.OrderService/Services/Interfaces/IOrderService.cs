using SagaSeminar.Shared.Models;

namespace SagaSeminar.Services.OrderService.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderModel> GetOrderById(Guid id);
        Task<OrderModel> GetOrderByTransactionId(Guid transactionId);
        Task CreateOrder(CreateOrderModel model);
        Task CancelOrder(Guid transactionId, string note);
        Task CompleteOrder(Guid transactionId);
        Task<ListResponseModel<OrderModel>> GetOrders(int skip, int take);
    }
}
