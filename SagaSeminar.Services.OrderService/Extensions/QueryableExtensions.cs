using SagaSeminar.Services.OrderService.Entities;
using SagaSeminar.Shared.Models;

namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<OrderModel> ToOrderModel(this IQueryable<OrderEntity> query)
        {
            return query.Select(e => new OrderModel
            {
                Amount = e.Amount,
                CreatedTime = e.CreatedTime,
                Customer = e.Customer,
                Id = e.Id,
                Status = e.Status,
                TransactionId = e.TransactionId,
                Note = e.Note,
            });
        }
    }
}
