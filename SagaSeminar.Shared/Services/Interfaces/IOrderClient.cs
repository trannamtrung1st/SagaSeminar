using SagaSeminar.Shared.Models;

namespace SagaSeminar.Shared.Services.Interfaces
{
    public interface IOrderClient
    {
        Task<ListResponseModel<OrderModel>> GetOrders(int skip, int take);
        Task<OrderModel> GetOrderDetails(Guid? id, Guid? transactionId);
        Task CreateOrder(CreateOrderModel model);
    }
}
